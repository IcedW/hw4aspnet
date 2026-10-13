import uk from './locales/uk.js'
import en from './locales/en.js'
import ru from './locales/ru.js'

// Підтримувані мови. Назви — рідними мовами, не залежать від поточної мови інтерфейсу.
// Коди збігаються з AppCultures на бекенді.
export const LANGUAGES = [
  { code: 'uk', name: 'Українська' },
  { code: 'en', name: 'English' },
  { code: 'ru', name: 'Русский' },
]

export const DEFAULT_LANGUAGE = 'uk'

const dictionaries = { uk, en, ru }
const STORAGE_KEY = 'kinoposhuk.lang'

function isSupported(code) {
  return LANGUAGES.some((l) => l.code === code)
}

function detectLanguage() {
  try {
    const saved = localStorage.getItem(STORAGE_KEY)
    if (isSupported(saved)) return saved
  } catch {
    // localStorage може бути недоступним — ігноруємо
  }
  const browser = (navigator.language || '').slice(0, 2).toLowerCase()
  return isSupported(browser) ? browser : DEFAULT_LANGUAGE
}

// Поточна мова зберігається на рівні модуля, щоб її могли читати і React-компоненти, і api.js
let currentLanguage = detectLanguage()

export function getLanguage() {
  return currentLanguage
}

export function setLanguage(code) {
  if (!isSupported(code)) return
  currentLanguage = code
  try {
    localStorage.setItem(STORAGE_KEY, code)
  } catch {
    // ignore
  }
}

export function languageName(code) {
  return LANGUAGES.find((l) => l.code === code)?.name ?? code
}

// translate('key', { name: 'value' }) — підставляє {name} у рядок.
// Якщо ключа немає в поточній мові — береться мова за замовчуванням, потім сам ключ.
export function translate(key, params = {}) {
  const template = dictionaries[currentLanguage]?.[key] ?? dictionaries[DEFAULT_LANGUAGE][key] ?? key
  return template.replace(/\{(\w+)\}/g, (_, name) => (name in params ? params[name] : `{${name}}`))
}
