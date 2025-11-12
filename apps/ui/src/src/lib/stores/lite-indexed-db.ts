import { readable, type Readable } from "svelte/store";
import { Dexie, type EntityTable } from "dexie";
import { LiteIndexedDbService } from "../../utils/lite/lite-indexed-db.service"
import { CONSTANTS } from "../../constants";

const indexedDb = new LiteIndexedDbService(CONSTANTS.DB_NAME)

export const liteDb = readable(indexedDb);

