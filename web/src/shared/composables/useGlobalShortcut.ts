import { onKeyStroke } from '@vueuse/core'

export type ShortcutHandler = (e: KeyboardEvent) => void

interface ShortcutOptions {
  /** Cmd on macOS, Ctrl elsewhere. Default: true */
  mod?: boolean
  shift?: boolean
  /** Trigger even when focus is inside an input/textarea/contenteditable. Default: false */
  allowInInputs?: boolean
  /** Call preventDefault on match. Default: true */
  preventDefault?: boolean
}

function isEditableTarget(target: EventTarget | null): boolean {
  if (!(target instanceof HTMLElement)) return false
  if (target.isContentEditable) return true
  const tag = target.tagName
  return tag === 'INPUT' || tag === 'TEXTAREA' || tag === 'SELECT'
}

/**
 * Cross-platform global keyboard shortcut. Uses VueUse `onKeyStroke`,
 * adds modifier-key handling (Cmd on mac, Ctrl elsewhere) and an
 * input-aware bypass so palette-style shortcuts still work while a user
 * is typing in a field.
 */
export function useGlobalShortcut(
  key: string,
  handler: ShortcutHandler,
  options: ShortcutOptions = {}
) {
  const { mod = true, shift = false, allowInInputs = true, preventDefault = true } = options
  const normalized = key.toLowerCase()

  onKeyStroke(
    (e) => e.key.toLowerCase() === normalized,
    (e) => {
      if (mod && !(e.metaKey || e.ctrlKey)) return
      if (!mod && (e.metaKey || e.ctrlKey)) return
      if (shift !== e.shiftKey) return
      if (!allowInInputs && isEditableTarget(e.target)) return
      if (preventDefault) e.preventDefault()
      handler(e)
    },
    { eventName: 'keydown' }
  )
}
