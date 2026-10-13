import { LANGUAGES } from '../i18n/index.js'
import { useI18n } from '../i18n/I18nContext.jsx'

export default function LanguageSwitcher() {
  const { language, setLanguage, t } = useI18n()

  return (
    <div className="lang-switcher" role="group" aria-label={t('lang.label')}>
      {LANGUAGES.map((l) => (
        <button
          key={l.code}
          type="button"
          className={l.code === language ? 'lang-btn active' : 'lang-btn'}
          onClick={() => setLanguage(l.code)}
          title={l.name}
        >
          {l.code.toUpperCase()}
        </button>
      ))}
    </div>
  )
}
