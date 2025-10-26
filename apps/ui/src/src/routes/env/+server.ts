import type { RequestHandler } from "./$types";
import { json } from "@sveltejs/kit";

export const GET: RequestHandler = () => {
  const injectedEnv: Record<string, string> = {};

  for (const key of Object.keys(process.env)) {
    if (key.startsWith("INJECTED")) {
      injectedEnv[key] = process.env[key] ?? "";
    }
  }

  return json(injectedEnv);
};