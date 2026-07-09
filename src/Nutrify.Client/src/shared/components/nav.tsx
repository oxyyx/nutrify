// Nav icons shared by the desktop Sidebar, the Navbar, and the mobile bottom
// tab bar. The destination list lives in nav-items.ts.

interface IconProps {
  className?: string;
}

export function IconDashboard({ className = "h-4 w-4 shrink-0" }: IconProps) {
  return (
    <svg className={className} fill="none" stroke="currentColor" strokeWidth={1.75} viewBox="0 0 24 24">
      <rect x="3" y="3" width="7.5" height="7.5" rx="1.25" />
      <rect x="13.5" y="3" width="7.5" height="7.5" rx="1.25" />
      <rect x="3" y="13.5" width="7.5" height="7.5" rx="1.25" />
      <rect x="13.5" y="13.5" width="7.5" height="7.5" rx="1.25" />
    </svg>
  );
}

export function IconIntake({ className = "h-4 w-4 shrink-0" }: IconProps) {
  return (
    <svg className={className} fill="none" stroke="currentColor" strokeWidth={1.75} viewBox="0 0 24 24">
      <path strokeLinecap="round" strokeLinejoin="round" d="M8 5H6a2 2 0 0 0-2 2v11a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V7a2 2 0 0 0-2-2h-2" />
      <rect x="8" y="3" width="8" height="4" rx="1" />
      <path strokeLinecap="round" d="M8 12h8M8 15.5h5" />
    </svg>
  );
}

export function IconFood({ className = "h-4 w-4 shrink-0" }: IconProps) {
  return (
    <svg className={className} fill="none" stroke="currentColor" strokeWidth={1.75} viewBox="0 0 24 24">
      <path strokeLinecap="round" strokeLinejoin="round" d="M12 4C10.7 9.7 6 12.2 6 16.5a6 6 0 0 0 12 0c0-4.3-4.7-6.8-6-12.5Z" />
      <path strokeLinecap="round" d="M12 11v6" />
    </svg>
  );
}

export function IconCategories({ className = "h-4 w-4 shrink-0" }: IconProps) {
  return (
    <svg className={className} fill="none" stroke="currentColor" strokeWidth={1.75} viewBox="0 0 24 24">
      <path strokeLinecap="round" strokeLinejoin="round" d="M7 7h.01M7 3h5.343a2 2 0 0 1 1.414.586l6.657 6.657a2 2 0 0 1 0 2.828l-5.343 5.343a2 2 0 0 1-2.828 0L5.586 11.757A2 2 0 0 1 5 10.343V5a2 2 0 0 1 2-2Z" />
    </svg>
  );
}

export function IconLogout({ className = "h-3.5 w-3.5 shrink-0" }: IconProps) {
  return (
    <svg className={className} fill="none" stroke="currentColor" strokeWidth={1.75} viewBox="0 0 24 24">
      <path strokeLinecap="round" strokeLinejoin="round" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 0 1-3 3H6a3 3 0 0 1-3-3V7a3 3 0 0 1 3-3h4a3 3 0 0 1 3 3v1" />
    </svg>
  );
}

export function LogoMark({ className = "shrink-0 text-primary" }: IconProps) {
  return (
    <svg width="16" height="16" fill="none" stroke="currentColor" strokeWidth={2.25} viewBox="0 0 24 24" className={className}>
      <path strokeLinecap="round" strokeLinejoin="round" d="M12 4C10.7 9.7 6 12.2 6 16.5a6 6 0 0 0 12 0c0-4.3-4.7-6.8-6-12.5Z" />
      <path strokeLinecap="round" d="M12 11v6" />
    </svg>
  );
}
