import js from '@eslint/js'
import pluginVue from 'eslint-plugin-vue'
import tsParser from '@typescript-eslint/parser'
import globals from 'globals'

// Flat config (ESLint v9+). Kept intentionally lean to match the project's
// minimal toolchain: the TypeScript *parser* is used so .ts/.vue script blocks
// parse, but the full type-aware @typescript-eslint plugin is deliberately not
// pulled in. Formatting is owned by Prettier, so we use eslint-plugin-vue's
// `essential` (correctness-only) tier rather than `recommended` to avoid
// fighting over line breaks / attribute layout.
export default [
  {
    ignores: ['dist/**', 'dev-dist/**', 'coverage/**', 'node_modules/**']
  },

  js.configs.recommended,
  ...pluginVue.configs['flat/essential'],

  // Shared rules + globals for all source files.
  {
    files: ['**/*.{ts,vue}'],
    languageOptions: {
      ecmaVersion: 'latest',
      sourceType: 'module',
      globals: {
        ...globals.browser
      }
    },
    rules: {
      // Single-word component files (Money.vue, Icon.vue) are intentional.
      'vue/multi-word-component-names': 'off',
      // TS handles undefined-variable checking; the core rule misfires on types.
      'no-undef': 'off',
      // tsc (noUnusedLocals/noUnusedParameters) owns unused-variable detection;
      // the core rule misfires on TS type-signature parameter names.
      'no-unused-vars': 'off',
      // Intl number formatting deliberately uses NBSP (U+00A0) in regex/strings
      // to normalize the locale group separator.
      'no-irregular-whitespace': [
        'error',
        { skipStrings: true, skipTemplates: true, skipComments: true, skipRegExps: true }
      ]
    }
  },

  // Plain TypeScript files: parse directly with the TS parser.
  {
    files: ['**/*.ts'],
    languageOptions: {
      parser: tsParser,
      parserOptions: {
        ecmaVersion: 'latest',
        sourceType: 'module'
      }
    }
  },

  // .vue files: vue-eslint-parser (set by eslint-plugin-vue) delegates the
  // <script lang="ts"> block to the TS parser.
  {
    files: ['**/*.vue'],
    languageOptions: {
      parserOptions: {
        parser: tsParser,
        ecmaVersion: 'latest',
        sourceType: 'module',
        extraFileExtensions: ['.vue']
      }
    }
  },

  // Test files also run under Node/Vitest globals.
  {
    files: ['**/*.{test,spec}.{ts,vue}', '**/__tests__/**'],
    languageOptions: {
      globals: {
        ...globals.node,
        ...globals.vitest
      }
    }
  }
]
