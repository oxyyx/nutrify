import { Link, useRouterState } from "@tanstack/react-router";
import { navItems } from "./nav-items";

/** Bottom tab bar shown on small screens; the Sidebar covers md and up. */
export function MobileNav() {
  const currentPath = useRouterState().location.pathname;

  return (
    <nav className="shrink-0 border-t border-gray-200 bg-white pb-[env(safe-area-inset-bottom)] md:hidden">
      <ul className="grid grid-cols-4">
        {navItems.map(({ to, shortLabel, Icon }) => {
          const isActive = currentPath.startsWith(to);
          return (
            <li key={to}>
              <Link
                to={to}
                className={`flex flex-col items-center gap-0.5 py-2 text-[11px] transition-colors ${
                  isActive ? "font-medium text-primary" : "text-gray-500 active:text-gray-800"
                }`}
              >
                <Icon className="h-5 w-5 shrink-0" />
                {shortLabel}
              </Link>
            </li>
          );
        })}
      </ul>
    </nav>
  );
}
