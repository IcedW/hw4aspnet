import { getLanguage, translate } from './i18n/index.js'

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5253/api'

// Усі запити йдуть із заголовком Accept-Language — бекенд відповідає обраною мовою
// (тексти фільмів і повідомлення валідації/помилок).
async function request(url, options = {}) {
  let response
  try {
    response = await fetch(url, {
      ...options,
      headers: { ...options.headers, 'Accept-Language': getLanguage() },
    })
  } catch {
    throw new Error(translate('errors.network'))
  }
  return handleResponse(response)
}

// ValidationProblem повертає { errors: { "Translations[0].Title": ["..."] } } — збираємо всі повідомлення
function collectValidationErrors(data) {
  if (!data?.errors) return null
  const messages = Object.values(data.errors).flat()
  return messages.length ? messages.join('\n') : null
}

async function handleResponse(response) {
  if (response.status === 204) return null

  const isJson = response.headers.get('content-type')?.includes('json')
  const data = isJson ? await response.json() : null

  if (!response.ok) {
    const message =
      data?.message || collectValidationErrors(data) || data?.title || translate('errors.api')
    const error = new Error(message)
    error.details = data
    throw error
  }

  return data
}

export function getMovies() {
  return request(`${API_URL}/movies`)
}

export function getMovie(id) {
  return request(`${API_URL}/movies/${id}`)
}

// Фільм з усіма перекладами — для форми редагування
export function getMovieTranslations(id) {
  return request(`${API_URL}/movies/${id}/translations`)
}

function toFormData(movie) {
  const formData = new FormData()
  formData.append('ReleaseYear', movie.releaseYear)
  movie.translations.forEach((tr, i) => {
    formData.append(`Translations[${i}].Culture`, tr.culture)
    formData.append(`Translations[${i}].Title`, tr.title)
    formData.append(`Translations[${i}].Director`, tr.director)
    formData.append(`Translations[${i}].Genre`, tr.genre)
    formData.append(`Translations[${i}].Description`, tr.description)
  })
  if (movie.poster) formData.append('Poster', movie.poster)
  return formData
}

export function createMovie(movie) {
  return request(`${API_URL}/movies`, { method: 'POST', body: toFormData(movie) })
}

export function updateMovie(id, movie) {
  return request(`${API_URL}/movies/${id}`, { method: 'PUT', body: toFormData(movie) })
}

export function deleteMovie(id) {
  return request(`${API_URL}/movies/${id}`, { method: 'DELETE' })
}

export function posterUrl(path) {
  if (!path) return null
  const origin = new URL(API_URL).origin
  return origin + path
}
