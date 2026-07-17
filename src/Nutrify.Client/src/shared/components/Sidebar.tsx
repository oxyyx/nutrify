import { Link, useRouterState } from "@tanstack/react-router";
import { useOidc } from "@/oidc";
import { getConfig } from "@/config";
import { IconLogout, LogoMark } from "./nav";
import { navItems } from "./nav-items";

// --- User section ---

function UserSection({
  decodedIdToken,
  onLogout,
  collapsed,
}: {
  decodedIdToken: { name: string; email: string };
  onLogout: () => void;
  collapsed: boolean;
}) {
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
      {!collapsed && (
        <p className="mt-1 px-2 text-[11px] text-gray-400">{getConfig().version}</p>
      )}
    </div>
  );
}

// --- Sidebar (desktop only; MobileNav covers small screens) ---

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
      className={`hidden flex-col border-r border-gray-200 bg-white transition-all duration-200 md:flex ${
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
          decodedIdToken={oidc.decodedIdToken}
          onLogout={() => oidc.logout({ redirectTo: "home" })}
          collapsed={collapsed}
        />
      ) : null}
    </aside>
  );
}
