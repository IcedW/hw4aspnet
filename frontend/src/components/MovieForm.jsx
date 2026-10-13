import { useState, useEffect } from 'react'
import { LANGUAGES, languageName } from '../i18n/index.js'
import { useI18n } from '../i18n/I18nContext.jsx'

const emptyTranslation = { title: '', director: '', genre: '', description: '' }
const FIELDS = ['title', 'director', 'genre', 'description']

function emptyTranslations() {
  return Object.fromEntries(LANGUAGES.map((l) => [l.code, { ...emptyTranslation }]))
}

const isFilled = (tr) => FIELDS.some((f) => tr[f].trim() !== '')
const isComplete = (tr) => FIELDS.every((f) => tr[f].trim() !== '')

export default function MovieForm({ initialMovie, onSubmit, onCancel, errorMessage }) {
  const { language, t } = useI18n()
  const [releaseYear, setReleaseYear] = useState('')
  const [poster, setPoster] = useState(null)
  const [translations, setTranslations] = useState(emptyTranslations)
  const [activeLang, setActiveLang] = useState(language)
  const [localError, setLocalError] = useState(null)

  useEffect(() => {
    const next = emptyTranslations()
    if (initialMovie) {
      initialMovie.translations.forEach((tr) => {
        if (next[tr.culture]) {
          next[tr.culture] = {
            title: tr.title,
            director: tr.director,
            genre: tr.genre,
            description: tr.description,
          }
        }
      })
      setReleaseYear(initialMovie.releaseYear)
    } else {
      setReleaseYear('')
    }
    setTranslations(next)
    setPoster(null)
    setLocalError(null)
  }, [initialMovie])

  function handleChange(e) {
    const { name, value } = e.target
    setTranslations((prev) => ({ ...prev, [activeLang]: { ...prev[activeLang], [name]: value } }))
  }

  function handleSubmit(e) {
    e.preventDefault()

    // Відправляємо лише ті мови, у яких щось введено; кожна має бути заповнена повністю
    const filled = LANGUAGES.filter((l) => isFilled(translations[l.code]))
    if (filled.length === 0) {
      setLocalError(t('form.errNoTranslation'))
      return
    }
    const incomplete = filled.find((l) => !isComplete(translations[l.code]))
    if (incomplete) {
      setActiveLang(incomplete.code)
      setLocalError(t('form.errIncomplete', { lang: languageName(incomplete.code) }))
      return
    }

    setLocalError(null)
    onSubmit({
      releaseYear,
      poster,
      translations: filled.map((l) => ({ culture: l.code, ...translations[l.code] })),
    })
  }

  const current = translations[activeLang]
  const error = localError || errorMessage

  return (
    <form className="movie-form" onSubmit={handleSubmit}>
      <h2>{initialMovie ? t('form.titleEdit') : t('form.titleNew')}</h2>
      {error && <p className="error">{error}</p>}

      <label>
        {t('form.year')}
        <input
          type="number"
          value={releaseYear}
          onChange={(e) => setReleaseYear(e.target.value)}
          required
          min={1888}
          max={2100}
        />
      </label>

      <fieldset className="translations">
        <legend>{t('form.translations')}</legend>
        <p className="hint">{t('form.translationsHint')}</p>

        <div className="tabs" role="tablist">
          {LANGUAGES.map((l) => (
            <button
              key={l.code}
              type="button"
              role="tab"
              aria-selected={l.code === activeLang}
              className={l.code === activeLang ? 'tab active' : 'tab'}
              onClick={() => setActiveLang(l.code)}
            >
              {l.name}
              {isFilled(translations[l.code]) && <span className="tab-mark"> ✓</span>}
            </button>
          ))}
        </div>

        <label>
          {t('form.title')}
          <input name="title" value={current.title} onChange={handleChange} maxLength={200} />
        </label>

        <label>
          {t('form.director')}
          <input name="director" value={current.director} onChange={handleChange} maxLength={100} />
        </label>

        <label>
          {t('form.genre')}
          <input name="genre" value={current.genre} onChange={handleChange} maxLength={50} />
        </label>

        <label>
          {t('form.description')}
          <textarea name="description" value={current.description} onChange={handleChange} maxLength={2000} rows={4} />
        </label>
      </fieldset>

      <label>
        {t('form.poster')} {initialMovie ? t('form.posterKeep') : ''}
        <input type="file" accept="image/*" onChange={(e) => setPoster(e.target.files[0] || null)} />
      </label>

      <div className="actions">
        <button type="submit">{t('form.save')}</button>
        <button type="button" onClick={onCancel}>{t('form.cancel')}</button>
      </div>
    </form>
  )
}
