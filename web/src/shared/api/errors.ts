import { type ProblemDetails } from './schemas'

export class ApiError extends Error {
  status: number
  problem?: ProblemDetails
  fieldErrors: Record<string, string[]>

  constructor(message: string, status: number, problem?: ProblemDetails) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.problem = problem
    this.fieldErrors = problem?.errors ?? {}
  }

  get isValidation(): boolean {
    return this.status === 400 && Object.keys(this.fieldErrors).length > 0
  }

  get isUnauthorized(): boolean {
    return this.status === 401
  }

  get isForbidden(): boolean {
    return this.status === 403
  }

  get isNotFound(): boolean {
    return this.status === 404
  }

  get isConflict(): boolean {
    return this.status === 409
  }

  get isServer(): boolean {
    return this.status >= 500
  }
}
