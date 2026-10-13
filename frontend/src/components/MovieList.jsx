import { posterUrl } from '../api.js'
import { languageName } from '../i18n/index.js'
import { useI18n } from '../i18n/I18nContext.jsx'

export default function MovieList({ movies, onEdit, onDelete }) {
  const { language, t } = useI18n()

  if (movies.length === 0) {
    return <p className="empty">{t('list.empty')}</p>
  }

  return (
    <div className="movie-grid">
      {movies.map((movie) => (
        <div className="movie-card" key={movie.id}>
          {movie.posterPath ? (
            <img src={posterUrl(movie.posterPath)} alt={movie.title} className="poster" />
          ) : (
            <div className="poster poster-placeholder">{t('list.noPoster')}</div>
          )}
          <div className="movie-info">
            <h3>{movie.title} ({movie.releaseYear})</h3>
            {movie.culture !== language && (
              <p className="fallback-note">{t('list.shownIn', { lang: languageName(movie.culture) })}</p>
            )}
            <p><strong>{t('list.director')}:</strong> {movie.director}</p>
            <p><strong>{t('list.genre')}:</strong> {movie.genre}</p>
            <p className="description">{movie.description}</p>
            <div className="actions">
              <button onClick={() => onEdit(movie)}>{t('list.edit')}</button>
              <button className="danger" onClick={() => onDelete(movie.id)}>{t('list.delete')}</button>
            </div>
          </div>
        </div>
      ))}
    </div>
  )
}
