import { useEffect, useMemo, useState } from "react";
import { HomePage } from "./pages/HomePage";
import { IntegerPage } from "./pages/IntegerPage";
import { NotFound, PropertyPage } from "./pages/PropertyPage";
import { openRelease } from "./release";
import { indexCorpus } from "./relations";
import { Link, useRoute } from "./router";
import type { PublicCorpus, PublicManifest } from "./types";

interface LoadedData {
  corpus: PublicCorpus;
  manifest: PublicManifest;
}

export function App() {
  const route = useRoute();
  const [loaded, setLoaded] = useState<LoadedData | null>(null);
  const [error, setError] = useState("");

  useEffect(() => {
    let active = true;
    Promise.all([
      fetch("/data/data.json").then(assertResponse).then((response) => response.arrayBuffer()),
      fetch("/data/manifest.json").then(assertResponse).then((response) => response.json() as Promise<PublicManifest>),
    ]).then(async ([dataBytes, manifest]) => {
      if (!active) return;
      const corpus = await openRelease(dataBytes, manifest);
      if (!active) return;
      setLoaded({ corpus, manifest });
    }).catch((reason: unknown) => {
      if (active) setError(reason instanceof Error ? reason.message : "the public release could not be opened.");
    });
    return () => { active = false; };
  }, []);

  const index = useMemo(() => loaded ? indexCorpus(loaded.corpus) : null, [loaded]);

  if (error) {
    return <div className="load-state" role="alert"><strong>release unavailable</strong><span>{error}</span></div>;
  }
  if (!index || !loaded) {
    return <div className="load-state" aria-live="polite"><strong>opening the public ledger</strong><span>checking the release seam…</span></div>;
  }

  const page = resolvePage(route.path, index);
  return (
    <>
      <a className="skip-link" href="#main">skip to content</a>
      <div className="grain" aria-hidden="true" />
      <header className="site-header">
        <Link href="/" className="wordmark">polyvalent</Link>
        <span className="site-descriptor">integer sociality index</span>
        <nav aria-label="featured integers">
          <Link href="/integer/153">153</Link>
          <Link href="/integer/343">343</Link>
          <Link href="/integer/137">137</Link>
        </nav>
      </header>
      {page}
      <footer className="site-footer">
        <span>read-only public release</span>
        <span>{loaded.manifest.releaseId} · schema {loaded.manifest.schemaVersion}</span>
        <span title={loaded.manifest.dataSha256}>digest {loaded.manifest.dataSha256.slice(0, 12)}…</span>
      </footer>
    </>
  );
}

function resolvePage(path: string, index: ReturnType<typeof indexCorpus>) {
  if (path === "/" || path === "") return <HomePage index={index} />;
  const integerMatch = path.match(/^\/integer\/([^/]+)\/?$/);
  if (integerMatch) {
    try {
      return <IntegerPage index={index} value={decodeURIComponent(integerMatch[1])} />;
    } catch {
      return <NotFound label="noncanonical integer" />;
    }
  }
  const propertyMatch = path.match(/^\/property\/([^/]+)\/?$/);
  if (propertyMatch) return <PropertyPage index={index} slug={decodeURIComponent(propertyMatch[1])} />;
  return <NotFound label="unknown coordinate" />;
}

function assertResponse(response: Response): Response {
  if (!response.ok) throw new Error(`release request failed with ${response.status}.`);
  return response;
}
