/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_PUBLIC_SUGGESTION_FORM_ID?: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
