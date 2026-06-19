interface ErrorStateProps {
  title?: string;
  message: string;
  onRetry?: () => void;
}

export function ErrorState({ title = "Something went wrong", message, onRetry }: ErrorStateProps) {
  return (
    <div className="flex flex-col items-center justify-center py-16 text-center">
      <div className="mb-4 flex h-10 w-10 items-center justify-center rounded-full bg-red-50">
        <svg
          className="h-5 w-5 text-red-500"
          fill="none"
          stroke="currentColor"
          strokeWidth={1.75}
          viewBox="0 0 24 24"
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            d="M12 9v4m0 4h.01M10.29 3.86 1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0Z"
          />
        </svg>
      </div>
      <p className="text-sm font-medium text-gray-800">{title}</p>
      <p className="mt-1 max-w-xs text-sm text-gray-500">{message}</p>
      {onRetry && (
        <button
          onClick={onRetry}
          className="mt-5 rounded-lg bg-primary px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-primary-dark"
        >
          Try again
        </button>
      )}
    </div>
  );
}
