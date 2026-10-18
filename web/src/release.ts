import type { PublicCorpus, PublicManifest } from "./types";

export async function openRelease(
  dataBytes: ArrayBuffer,
  manifest: PublicManifest,
): Promise<PublicCorpus> {
  const digest = await sha256Hex(dataBytes);
  if (digest !== manifest.dataSha256.toLowerCase()) {
    throw new Error("the public release digest does not match its manifest.");
  }

  const corpus = JSON.parse(new TextDecoder().decode(dataBytes)) as PublicCorpus;
  if (corpus.releaseId !== manifest.releaseId || corpus.schemaVersion !== manifest.schemaVersion) {
    throw new Error("the public release manifest does not match its data payload.");
  }

  return corpus;
}

async function sha256Hex(bytes: ArrayBuffer): Promise<string> {
  const digest = await globalThis.crypto.subtle.digest("SHA-256", bytes);
  return [...new Uint8Array(digest)]
    .map((byte) => byte.toString(16).padStart(2, "0"))
    .join("");
}
