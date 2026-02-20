# MCP for Payobills
## Overview
This is the Model Context Protocol (MCP) application for the Payobills project.

This MCP server exposes GraphQL queries as tools for an LLM to use data from payobills.
This does not expose mutations as tools until the MCP server has been thoroughly tested. 

## Running the Application
```bash
$ GRAPHQL_ENDPOINT=http://localhost:31000/graphql \
  npx @modelcontextprotocol/inspector node index.mjs
```

