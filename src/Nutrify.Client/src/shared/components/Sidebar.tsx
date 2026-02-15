import { Link, useRouterState } from "@tanstack/react-router";
import { useOidc } from "@/oidc";

// --- Icon components (outline, 16×16 render) ---

function IconDashboard() {
  return (
    <svg className="h-4 w-4 shrink-0" fill="none" stroke="currentColor" strokeWidth={1.75} viewBox="0 0 24 24">
      <rect x="3" y="3" width="7.5" height="7.5" rx="1.25" />
      <rect x="13.5" y="3" width="7.5" height="7.5" rx="1.25" />
      <rect x="3" y="13.5" width="7.5" height="7.5" rx="1.25" />
      <rect x="13.5" y="13.5" width="7.5" height="7.5" rx="1.25" />
    </svg>
  );
}

function IconIntake() {
  return (
    <svg className="h-4 w-4 shrink-0" fill="none" stroke="currentColor" strokeWidth={1.75} viewBox="0 0 24 24">
      <path strokeLinecap="round" strokeLinejoin="round" d="M8 5H6a2 2 0 0 0-2 2v11a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V7a2 2 0 0 0-2-2h-2" />
      <rect x="8" y="3" width="8" height="4" rx="1" />
      <path strokeLinecap="round" d="M8 12h8M8 15.5h5" />
    </svg>
  );
}

function IconFood() {
  return (
    <svg className="h-4 w-4 shrink-0" fill="none" stroke="currentColor" strokeWidth={1.75} viewBox="0 0 24 24">
      <path strokeLinecap="round" strokeLinejoin="round" d="M12 4C10.7 9.7 6 12.2 6 16.5a6 6 0 0 0 12 0c0-4.3-4.7-6.8-6-12.5Z" />
      <path strokeLinecap="round" d="M12 11v6" />
    </svg>
  );
}

function IconCategories() {
  return (
    <svg className="h-4 w-4 shrink-0" fill="none" stroke="currentColor" strokeWidth={1.75} viewBox="0 0 24 24">
      <path strokeLinecap="round" strokeLinejoin="round" d="M7 7h.01M7 3h5.343a2 2 0 0 1 1.414.586l6.657 6.657a2 2 0 0 1 0 2.828l-5.343 5.343a2 2 0 0 1-2.828 0L5.586 11.757A2 2 0 0 1 5 10.343V5a2 2 0 0 1 2-2Z" />
    </svg>
  );
}

function IconLogout() {
  return (
    <svg className="h-3.5 w-3.5 shrink-0" fill="none" stroke="currentColor" strokeWidth={1.75} viewBox="0 0 24 24">
      <path strokeLinecap="round" strokeLinejoin="round" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 0 1-3 3H6a3 3 0 0 1-3-3V7a3 3 0 0 1 3-3h4a3 3 0 0 1 3 3v1" />
    </svg>
  );
}

function LogoMark() {
  return (
    <svg width="16" height="16" fill="none" stroke="currentColor" strokeWidth={2.25} viewBox="0 0 24 24" className="shrink-0 text-primary">
      <path strokeLinecap="round" strokeLinejoin="round" d="M12 4C10.7 9.7 6 12.2 6 16.5a6 6 0 0 0 12 0c0-4.3-4.7-6.8-6-12.5Z" />
      <path strokeLinecap="round" d="M12 11v6" />
    </svg>
  );
}

// --- Nav items ---

const navItems = [
  { to: "/dashboard", label: "Dashboard", Icon: IconDashboard },
  { to: "/intake", label: "Intake Log", Icon: IconIntake },
  { to: "/food-items", label: "Food Items", Icon: IconFood },
  { to: "/categories", label: "Categories", Icon: IconCategories },
] as const;

// --- User section ---

function UserSection({
  tokens,
  onLogout,
  collapsed,
}: {
  tokens: { decodedIdToken: { name: string; email: string } };
  onLogout: () => void;
  collapsed: boolean;
}) {
  const { decodedIdToken } = tokens;
  const initials = decodedIdToken.name?.charAt(0)?.toUpperCase() ?? "?";

  return (
    <div className="border-t border-gray-200 p-2">
      {!collapsed && (
        <div className="mb-1 flex items-center gap-2.5 rounded-md px-2 py-1.5">
          <div className="flex h-6 w-6 shrink-0 items-center justify-center rounded-full bg-primary/10 text-[11px] font-semibold text-primary">
            {initials}
          </div>
          <div className="flex-1 truncate">
            <p className="truncate text-xs font-medium text-gray-800">{decodedIdToken.name}</p>
            <p className="truncate text-[11px] leading-tight text-gray-400">{decodedIdToken.email}</p>
          </div>
        </div>
      )}
      <button
        onClick={onLogout}
        title={collapsed ? "Sign out" : undefined}
        className={`flex items-center gap-2 rounded-md px-2 py-1.5 text-xs text-gray-500 transition-colors hover:bg-gray-100 hover:text-gray-700 ${
          collapsed ? "mx-auto w-8 justify-center" : "w-full"
        }`}
      >
        <IconLogout />
        {!collapsed && "Sign out"}
      </button>
    </div>
  );
}

// --- Sidebar ---

export function Sidebar({
  collapsed,
  onToggle,
}: {
  collapsed: boolean;
  onToggle: () => void;
}) {
  const routerState = useRouterState();
  const currentPath = routerState.location.pathname;
  const oidc = useOidc();

  return (
    <aside
      className={`flex flex-col border-r border-gray-200 bg-white transition-all duration-200 ${
        collapsed ? "w-14" : "w-56"
      }`}
    >
      {/* Header */}
      <div
        className={`flex h-14 shrink-0 items-center border-b border-gray-200 ${
          collapsed ? "justify-center px-2" : "justify-between px-3"
        }`}
      >
        {!collapsed && (
          <div className="flex items-center gap-2">
            <LogoMark />
            <span className="text-sm font-semibold tracking-tight text-gray-900">Nutrify</span>
          </div>
        )}
        <button
          onClick={onToggle}
          title={collapsed ? "Expand" : "Collapse"}
          className={`flex h-7 w-7 items-center justify-center rounded-md text-gray-400 transition-colors hover:bg-gray-100 hover:text-gray-600 ${
            collapsed ? "rotate-180" : ""
          }`}
        >
          <svg className="h-3.5 w-3.5" fill="none" stroke="currentColor" strokeWidth={2} viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" d="M15 19l-7-7 7-7" />
          </svg>
        </button>
      </div>

      {/* Navigation */}
      <nav className="flex-1 overflow-y-auto p-2">
        <ul className="space-y-0.5">
          {navItems.map(({ to, label, Icon }) => {
            const isActive = currentPath.startsWith(to);
            return (
              <li key={to}>
                <Link
                  to={to}
                  title={collapsed ? label : undefined}
                  className={`flex items-center rounded-md px-2.5 py-2 text-sm transition-colors ${
                    collapsed ? "justify-center" : "gap-2.5"
                  } ${
                    isActive
                      ? "bg-primary/[0.07] font-medium text-primary"
                      : "font-normal text-gray-600 hover:bg-gray-100 hover:text-gray-900"
                  }`}
                >
                  <Icon />
                  {!collapsed && label}
                </Link>
              </li>
            );
          })}
        </ul>
      </nav>

      {/* User */}
      {oidc.isUserLoggedIn ? (
        <UserSection
          tokens={oidc.oidcTokens}
          onLogout={() => oidc.logout({ redirectTo: "home" })}
          collapsed={collapsed}
        />
      ) : null}
    </aside>
  );
}
