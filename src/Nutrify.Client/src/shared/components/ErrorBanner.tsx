interface ErrorBannerProps {
  message: string;
  className?: string;
}

export function ErrorBanner({ message, className = "" }: ErrorBannerProps) {
  return (
    <div
      role="alert"
      className={`flex items-start gap-2 rounded-md border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700 ${className}`}
    >
      <svg
        className="mt-0.5 h-4 w-4 shrink-0"
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
      <span>{message}</span>
    </div>
  );
}
