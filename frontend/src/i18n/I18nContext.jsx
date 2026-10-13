import { createContext, useContext, useEffect, useMemo, useState } from 'react'
import { getLanguage, setLanguage as storeLanguage, translate } from './index.js'

const I18nContext = createContext(null)

export function I18nProvider({ children }) {
  const [language, setLanguageState] = useState(getLanguage())

  function setLanguage(code) {
    storeLanguage(code) // оновлює модуль (api.js бере мову звідти) і localStorage
    setLanguageState(getLanguage())
  }

  // <html lang> та заголовок вкладки
  useEffect(() => {
    document.documentElement.lang = language
    document.title = translate('app.title')
  }, [language])

  const value = useMemo(
    () => ({ language, setLanguage, t: (key, params) => translate(key, params) }),
    // t залежить від language: нове значення language -> нова функція -> перерисовка споживачів
    [language],
  )

  return <I18nContext.Provider value={value}>{children}</I18nContext.Provider>
}

export function useI18n() {
  const ctx = useContext(I18nContext)
  if (!ctx) throw new Error('useI18n must be used inside <I18nProvider>')
  return ctx
}
