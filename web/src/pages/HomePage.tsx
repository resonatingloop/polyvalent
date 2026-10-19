import { Link } from "../router";
import { SearchForm } from "../SearchForm";
import type { CorpusIndex } from "../types";

export function HomePage({ index }: { index: CorpusIndex }) {
  return (
    <main id="main" className="home-page">
      <section className="hero">
        <div className="hero-index" aria-hidden="true">001—2000</div>
        <div className="hero-copy reveal one">
          <p className="eyebrow">a social network(ing graph) for integers</p>
          <h1>numbers have<br /><em>mutuals.</em></h1>
          <p className="hero-deck">
            enter an integer. meet its public properties, inspect the evidence,
            then follow the exact reason it knows another number.
          </p>
        </div>
        <div className="hero-orbit reveal two" aria-hidden="true">
          <div className="orbit orbit-one"><span>153</span></div>
          <div className="orbit orbit-two"><span>343</span></div>
          <div className="orbit-label">friedman</div>
        </div>
        <div className="hero-search reveal three">
          <SearchForm />
        </div>
        <a className="hero-continuation reveal four" href="#dispatch">
          <span>next register</span>
          <strong>public dispatch</strong>
          <span className="hero-continuation-mark" aria-hidden="true">↓</span>
        </a>
      </section>

      <section id="dispatch" className="dispatch-grid" aria-labelledby="dispatch-heading">
        <div className="section-rule"><span>public dispatch / {index.corpus.releaseId}</span></div>
        <h2 id="dispatch-heading">three apertures into the corpus</h2>
        <div className="dispatch-plates">
          <Link className="dispatch-card primary" href="/integer/153">
            <span className="dispatch-number">153</span>
            <span className="dispatch-label">a crowded little hinge</span>
            <span className="dispatch-note">triangular · narcissistic · friedman</span>
          </Link>
          <Link className="dispatch-card" href="/integer/343">
            <span className="dispatch-number">343</span>
            <span className="dispatch-label">the orderly acquaintance</span>
            <span className="dispatch-note">cube · palindromic · nice friedman</span>
          </Link>
          <Link className="dispatch-card" href="/integer/137">
            <span className="dispatch-number">137</span>
            <span className="dispatch-label">the charged acquaintance</span>
            <span className="dispatch-note">prime · twin prime · scientific context</span>
          </Link>
        </div>
      </section>

      <section className="how-it-works">
        <p className="eyebrow">the etiquette of the thing</p>
        <div>
          <h2>affiliation is not implication.</h2>
          <p>
            every visible relation passes through a named, published property.
            absence is only absence when a recorded bounded evaluation warrants it.
            everything else remains honestly unassessed.
          </p>
        </div>
      </section>
    </main>
  );
}
