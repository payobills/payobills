#!/usr/bin/env node

import fetch from "node-fetch";
import {
  getIntrospectionQuery,
  buildClientSchema,
  isNonNullType,
  isListType,
  isScalarType,
  isEnumType,
  isInputObjectType,
  isObjectType,
  isInterfaceType,
  isUnionType,
} from "graphql";

import { Server } from "@modelcontextprotocol/sdk/server/index.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import {
  ListToolsRequestSchema,
  CallToolRequestSchema,
} from "@modelcontextprotocol/sdk/types.js";

/* ================= CONFIG ================= */

const ENDPOINT = process.env.GRAPHQL_ENDPOINT;
if (!ENDPOINT) {
  console.error("❌ Set GRAPHQL_ENDPOINT env variable");
  process.exit(1);
}

const AUTH_HEADER = process.env.AUTH_HEADER || null;
const MAX_SELECTION_DEPTH = 3;

/* ================= INTROSPECTION ================= */

async function introspect() {
  const res = await fetch(ENDPOINT, {
    method: "POST",
    headers: {
      "content-type": "application/json",
      ...(AUTH_HEADER ? { authorization: AUTH_HEADER } : {}),
    },
    body: JSON.stringify({ query: getIntrospectionQuery() }),
  });

  const json = await res.json();
  if (json.errors) throw new Error(JSON.stringify(json.errors, null, 2));
  return buildClientSchema(json.data);
}

/* ================= TYPE UTILITIES ================= */

function unwrapAll(type) {
  let required = false;
  let list = false;

  while (true) {
    if (isNonNullType(type)) {
      required = true;
      type = type.ofType;
      continue;
    }
    if (isListType(type)) {
      list = true;
      type = type.ofType;
      continue;
    }
    break;
  }

  return { type, required, list };
}

function scalarToJson(name) {
  switch (name) {
    case "Int":
    case "Float":
      return { type: "number" };
    case "Boolean":
      return { type: "boolean" };
    default:
      return { type: "string" };
  }
}

/* ================= INPUT MAPPING ================= */

function mapInput(type) {
  const { type: t, required, list } = unwrapAll(type);

  let schema;

  if (isScalarType(t)) {
    schema = scalarToJson(t.name);
  } else if (isEnumType(t)) {
    schema = {
      type: "string",
      enum: t.getValues().map(v => v.name),
    };
  } else if (isInputObjectType(t)) {
    const fields = t.getFields();
    const properties = {};
    const requiredFields = [];

    for (const name in fields) {
      const mapped = mapInput(fields[name].type);
      properties[name] = mapped.schema;
      if (mapped.required) requiredFields.push(name);
    }

    schema = {
      type: "object",
      properties,
      required: requiredFields,
    };
  } else {
    schema = { type: "string" };
  }

  if (list) {
    schema = { type: "array", items: schema };
  }

  return {
    schema,
    required,
    gqlType: type.toString(), // preserves full GraphQL type signature
  };
}

/* ================= SELECTION GENERATION ================= */

function buildSelection(type, depth = 0, visited = new Set()) {
  const { type: t } = unwrapAll(type);

  if (depth >= MAX_SELECTION_DEPTH) return "";

  if (isScalarType(t) || isEnumType(t)) return "";

  if (visited.has(t.name)) return "";
  visited.add(t.name);

  if (isObjectType(t) || isInterfaceType(t)) {
    const fields = t.getFields();
    const selections = [];

    for (const fieldName in fields) {
      const field = fields[fieldName];

      const subSelection = buildSelection(
        field.type,
        depth + 1,
        new Set(visited)
      );

      if (subSelection) {
        selections.push(`${fieldName} { ${subSelection} }`);
      } else {
        selections.push(fieldName);
      }
    }

    return selections.join(" ");
  }

  if (isUnionType(t)) {
    return t
      .getTypes()
      .map(
        subtype =>
          `... on ${subtype.name} { ${buildSelection(
            subtype,
            depth + 1,
            new Set(visited)
          )} }`
      )
      .join(" ");
  }

  return "";
}

/* ================= TOOL GENERATION ================= */

function generateTools(schema) {
  const tools = [];

  const roots = [
    { type: schema.getQueryType(), op: "query" },
    // Ignore mutations on MCP
    // TODO: Put back once thoroughly tested
    //{ type: schema.getMutationType(), op: "mutation" },
  ].filter(r => r.type);

  for (const root of roots) {
    const fields = root.type.getFields();

    for (const fieldName in fields) {
      const field = fields[fieldName];

      const properties = {};
      const required = [];
      const variableDefs = [];
      const variableUses = [];

      for (const arg of field.args) {
        const mapped = mapInput(arg.type);

        properties[arg.name] = mapped.schema;
        if (mapped.required) required.push(arg.name);

        variableDefs.push(`$${arg.name}: ${mapped.gqlType}`);
        variableUses.push(`${arg.name}: $${arg.name}`);
      }

      properties.__selection = {
        type: "string",
        description:
          "Optional GraphQL selection set override. Example: id name posts { id title }",
      };

      const selection = buildSelection(field.type);

      tools.push({
        name: field.name,
        description: field.description || `${root.op} ${field.name}`,
        operation: root.op,
        inputSchema: {
          type: "object",
          properties,
          required,
        },
        variableDefs,
        variableUses,
        selection,
      });
    }
  }

  return tools;
}

/* ================= EXECUTION ================= */

async function execute(tool, args = {}) {
  const { __selection, ...realArgs } = args;

  const hasArgs = tool.variableDefs.length > 0;

  const operationHeader = hasArgs
    ? `${tool.operation} ${tool.name}(${tool.variableDefs.join(", ")})`
    : `${tool.operation} ${tool.name}`;

  const fieldCall = hasArgs
    ? `${tool.name}(${tool.variableUses.join(", ")})`
    : `${tool.name}`;

  const selection =
    __selection && __selection.trim().length > 0
      ? __selection
      : tool.selection;

  const isScalarReturn = !selection || selection.trim().length === 0;

  const query = `
    ${operationHeader} {
      ${fieldCall} ${
        isScalarReturn ? "" : `{ ${selection} }`
      }
    }
  `;

  const res = await fetch(ENDPOINT, {
    method: "POST",
    headers: {
      "content-type": "application/json",
      ...(AUTH_HEADER ? { authorization: AUTH_HEADER } : {}),
    },
    body: JSON.stringify({
      query,
      variables: hasArgs ? realArgs : undefined,
    }),
  });

  return await res.json();
}

/* ================= MCP SERVER ================= */

async function start() {
  console.error("🔎 Introspecting schema...");
  const schema = await introspect();

  const tools = generateTools(schema);
  console.error(`✅ Discovered ${tools.length} tools`);

  const server = new Server(
    { name: "graphql-supergraph-mcp", version: "1.0.0" },
    { capabilities: { tools: {} } }
  );

  server.setRequestHandler(ListToolsRequestSchema, async () => ({
    tools: tools.map(t => ({
      name: t.name,
      description: t.description,
      inputSchema: t.inputSchema,
    })),
  }));

  server.setRequestHandler(CallToolRequestSchema, async request => {
    const { name, arguments: args } = request.params;

    const tool = tools.find(t => t.name === name);
    if (!tool) throw new Error(`Tool not found: ${name}`);

    const result = await execute(tool, args || {});

    return {
      content: [
        {
          type: "text",
          text: JSON.stringify(result, null, 2),
        },
      ],
    };
  });

  await server.connect(new StdioServerTransport());
  console.error("🚀 MCP server ready");
}

start().catch(err => {
  console.error(err);
  process.exit(1);
});

