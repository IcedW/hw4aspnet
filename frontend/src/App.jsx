import { useEffect, useState } from 'react'
import MovieList from './components/MovieList.jsx'
import MovieForm from './components/MovieForm.jsx'
import LanguageSwitcher from './components/LanguageSwitcher.jsx'
import { getMovies, getMovieTranslations, createMovie, updateMovie, deleteMovie } from './api.js'
import { useI18n } from './i18n/I18nContext.jsx'

export default function App() {
  const { language, t } = useI18n()
  const [movies, setMovies] = useState([])
  const [loading, setLoading] = useState(true)
  const [loadError, setLoadError] = useState(null)
  const [showForm, setShowForm] = useState(false)
  const [editingMovie, setEditingMovie] = useState(null)
  const [formError, setFormError] = useState(null)
  const [githubUser, setGithubUser] = useState(null)
  const [authLoading, setAuthLoading] = useState(true)
  const apiBase = (import.meta.env.VITE_API_URL || 'http://localhost:5253/api').replace(/\/api\/?$/, '')

  function loadMovies() {
    setLoading(true)
    getMovies()
      .then((data) => {
        setMovies(data)
        setLoadError(null)
      })
      .catch((err) => setLoadError(err.message))
      .finally(() => setLoading(false))
  }

  useEffect(() => {
    fetch(`${apiBase}/auth/me`, { credentials: 'include' })
      .then((response) => response.ok ? response.json() : null)
      .then(setGithubUser)
      .catch(() => setGithubUser(null))
      .finally(() => setAuthLoading(false))
  }, [])

  function loginWithGithub() {
    window.location.href = `${apiBase}/auth/github`
  }

  async function logout() {
    await fetch(`${apiBase}/auth/logout`, {
      method: 'POST',
      credentials: 'include',
    })
    setGithubUser(null)
  }

  // Список перезавантажується при зміні мови: бекенд повертає тексти фільмів обраною мовою
  useEffect(loadMovies, [language])

  function openCreateForm() {
    setEditingMovie(null)
    setFormError(null)
    setShowForm(true)
  }

  // Для редагування потрібні всі переклади фільму, а не лише поточна мова
  async function openEditForm(movie) {
    try {
      const full = await getMovieTranslations(movie.id)
      setEditingMovie(full)
      setFormError(null)
      setShowForm(true)
    } catch (err) {
      alert(err.message)
    }
  }

  async function handleSubmit(form) {
    try {
      if (editingMovie) {
        await updateMovie(editingMovie.id, form)
      } else {
        await createMovie(form)
      }
      setShowForm(false)
      loadMovies()
    } catch (err) {
      setFormError(err.message)
    }
  }

  async function handleDelete(id) {
    if (!confirm(t('app.confirmDelete'))) return
    try {
      await deleteMovie(id)
      loadMovies()
    } catch (err) {
      alert(err.message)
    }
  }

  return (
    <div className="app">
      <header>
        <h1>{t('app.title')}</h1>
        <div className="header-controls">
          <LanguageSwitcher />
          {!authLoading && (githubUser ? (
            <div className="github-account">
              <span>GitHub: {githubUser.username}</span>
              <button className="github-button" onClick={logout}>Вийти</button>
            </div>
          ) : (
            <button className="github-button" onClick={loginWithGithub}>
              <svg viewBox="0 0 24 24" aria-hidden="true"><path fill="currentColor" d="M12 .9a11.1 11.1 0 0 0-3.51 21.63c.55.1.76-.24.76-.53v-2.08c-3.1.67-3.76-1.32-3.76-1.32-.5-1.29-1.24-1.63-1.24-1.63-1.01-.69.08-.68.08-.68 1.12.08 1.71 1.15 1.71 1.15 1 .1.1.6 2.54.76.1-.72.39-1.22.7-1.5-2.48-.28-5.09-1.24-5.09-5.52 0-1.22.44-2.22 1.15-3-.12-.28-.5-1.42.11-2.96 0 0 .94-.3 3.05 1.14a10.6 10.6 0 0 1 5.55 0c2.11-1.44 3.05-1.14 3.05-1.14.61 1.54.23 2.68.11 2.96.72.78 1.15 1.78 1.15 3 0 4.29-2.61 5.24-5.1 5.51.4.35.75 1.03.75 2.08V22c0 .29.2.63.76.53A11.1 11.1 0 0 0 12 .9Z"/></svg>
              Увійти під акаунтом Github
            </button>
          ))}
          {!showForm && <button onClick={openCreateForm}>{t('app.addMovie')}</button>}
        </div>
      </header>

      {loading && <p>{t('app.loading')}</p>}
      {loadError && <p className="error">{t('app.loadError', { error: loadError })}</p>}

      {!loading && !loadError && !showForm && (
        <MovieList movies={movies} onEdit={openEditForm} onDelete={handleDelete} />
      )}

      {showForm && (
        <MovieForm
          initialMovie={editingMovie}
          onSubmit={handleSubmit}
          onCancel={() => setShowForm(false)}
          errorMessage={formError}
        />
      )}
    </div>
  )
}
