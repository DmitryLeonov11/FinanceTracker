import { onKeyStroke } from '@vueuse/core'

export type ShortcutHandler = (e: KeyboardEvent) => void

interface ShortcutOptions {
  mod?: boolean
  shift?: boolean
  allowInInputs?: boolean
  preventDefault?: boolean
}

function isEditableTarget(target: EventTarget | null): boolean {
  if (!(target instanceof HTMLElement)) return false
  if (target.isContentEditable) return true
  const tag = target.tagName
  return tag === 'INPUT' || tag === 'TEXTAREA' || tag === 'SELECT'
}

// Cmd на mac, Ctrl везде остальные; по умолчанию срабатывает даже когда фокус в поле ввода,
// чтобы шорткут палитры команд не терялся во время набора текста.
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
