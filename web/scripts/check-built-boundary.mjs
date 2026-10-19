import { readFile, readdir } from "node:fs/promises";
import { extname } from "node:path";

const files = await builtFiles(new URL("../dist/", import.meta.url));
if (files.length === 0) throw new Error("web/dist is empty; build before checking its boundary");

const forbidden = [
  /[a-z0-9._%+-]+@gmail\.com/i,
  /microsoft\.data\.sqlite/i,
  /better-sqlite3/i,
  /polyvalent\.data/i,
  /databasepathresolver/i,
  /privatenotes/i,
  /sourcecaptureid/i,
];

for (const file of files) {
  const contents = await readFile(file, "utf8");
  for (const pattern of forbidden) {
    if (pattern.test(contents)) throw new Error(`${file.pathname} contains forbidden built-boundary token ${pattern}`);
  }
}

console.log(`built public boundary ok: ${files.length} inspectable files, no recipient address, credential, or private runtime tokens`);

async function builtFiles(directory) {
  const entries = await readdir(directory, { withFileTypes: true });
  const nested = await Promise.all(entries.map(async (entry) => {
    const child = new URL(`${entry.name}${entry.isDirectory() ? "/" : ""}`, directory);
    if (entry.isDirectory()) return builtFiles(child);
    return [child];
  }));
  return nested.flat().filter((file) => [".css", ".html", ".js", ".json", ".map"].includes(extname(file.pathname)));
}
