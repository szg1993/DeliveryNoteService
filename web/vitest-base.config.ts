import { defineConfig } from 'vitest/config';

export default defineConfig({
  test: {
    // The first routed test in each file lazy loads its page while the whole suite runs in parallel,
    // which can take over 5 s. Keep this above the 5 s wait in src/testing/http.ts.
    testTimeout: 15000,
  },
});
