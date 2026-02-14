import { Link, useRouterState } from "@tanstack/react-router";
import { useOidc } from "@/oidc";

const navItems = [
  { to: "/dashboard", label: "Dashboard", icon: "📊" },
  { to: "/intake", label: "Intake Log", icon: "📝" },
  { to: "/food-items", label: "Food Items", icon: "🍎" },
  { to: "/categories", label: "Categories", icon: "📁" },
] as const;

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
  return (
    <div className="border-t border-gray-200 p-4">
      <div
        className={`flex items-center rounded-lg px-3 py-2 ${collapsed ? "justify-center" : "gap-3"}`}
      >
        <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-primary/10 text-sm font-medium text-primary">
          {decodedIdToken.name?.charAt(0)?.toUpperCase() ?? "?"}
        </div>
        {!collapsed && (
          <div className="flex-1 truncate">
            <p className="truncate text-sm font-medium text-gray-900">
              {decodedIdToken.name}
            </p>
            <p className="truncate text-xs text-gray-500">
              {decodedIdToken.email}
            </p>
          </div>
        )}
      </div>
      <button
        onClick={onLogout}
        title={collapsed ? "Logout" : undefined}
        className={`mt-2 flex items-center justify-center rounded-lg px-2 py-1.5 text-xs font-medium text-gray-700 transition-colors hover:bg-gray-100 ${collapsed ? "w-8 mx-auto" : "w-full gap-2"}`}
      >
        <svg
          className="h-4 w-4 shrink-0"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth={2}
            d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1"
          />
        </svg>
        {!collapsed && "Logout"}
      </button>
    </div>
  );
}

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
      className={`flex flex-col border-r border-gray-200 bg-white transition-all duration-300 ${collapsed ? "w-16" : "w-64"}`}
    >
      <div className="flex h-16 items-center justify-between border-b border-gray-200 px-4">
        {!collapsed && (
          <span className="text-lg font-semibold text-primary">Nutrify</span>
        )}
        <button
          onClick={onToggle}
          className={`flex h-8 w-8 shrink-0 items-center justify-center rounded-md text-gray-500 transition-colors hover:bg-gray-100 hover:text-gray-700 ${collapsed ? "mx-auto" : ""}`}
        >
          <svg
            className={`h-4 w-4 transition-transform duration-300 ${collapsed ? "rotate-180" : ""}`}
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M11 19l-7-7 7-7m8 14l-7-7 7-7"
            />
          </svg>
        </button>
      </div>
      <nav className="flex-1 p-4">
        <ul className="space-y-1">
          {navItems.map((item) => {
            const isActive = currentPath.startsWith(item.to);
            return (
              <li key={item.to}>
                <Link
                  to={item.to}
                  title={collapsed ? item.label : undefined}
                  className={`flex items-center rounded-lg px-3 py-2 text-sm font-medium transition-colors ${collapsed ? "justify-center" : "gap-3"} ${
                    isActive
                      ? "bg-primary/10 text-primary"
                      : "text-gray-700 hover:bg-gray-100"
                  }`}
                >
                  <span className="shrink-0">{item.icon}</span>
                  {!collapsed && item.label}
                </Link>
              </li>
            );
          })}
        </ul>
      </nav>
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
