import { useEffect, useState, type FormEvent } from "react";
import { canonicalInteger } from "./integer";
import { navigate } from "./router";

export function SearchForm({ compact = false, initial = "153" }: { compact?: boolean; initial?: string }) {
  const [value, setValue] = useState(initial);
  const [error, setError] = useState("");

  useEffect(() => {
    setValue(initial);
    setError("");
  }, [initial]);

  const submit = (event: FormEvent) => {
    event.preventDefault();
    try {
      const canonical = canonicalInteger(value);
      setError("");
      navigate(`/integer/${encodeURIComponent(canonical)}`);
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : "that integer could not be opened.");
    }
  };

  return (
    <form className={compact ? "search-form compact" : "search-form"} onSubmit={submit} noValidate>
      <label htmlFor={compact ? "integer-search-compact" : "integer-search"}>
        {compact ? "open another integer" : "which integer should we ask about?"}
      </label>
      <div className="search-row">
        <span aria-hidden="true">№</span>
        <input
          id={compact ? "integer-search-compact" : "integer-search"}
          value={value}
          onChange={(event) => setValue(event.target.value)}
          inputMode="numeric"
          autoComplete="off"
          spellCheck={false}
          aria-describedby={error ? "integer-search-error" : undefined}
        />
        <button type="submit">enter the network</button>
      </div>
      <p id="integer-search-error" className="form-error" aria-live="polite">{error}</p>
    </form>
  );
}
