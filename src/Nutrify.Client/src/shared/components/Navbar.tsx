import { useOidc } from "@/oidc";
import { IconLogout, LogoMark } from "./nav";

export function Navbar() {
  const oidc = useOidc();

  const today = new Date().toLocaleDateString(undefined, {
    weekday: "long",
    month: "long",
    day: "numeric",
  });
  const todayShort = new Date().toLocaleDateString(undefined, {
    month: "short",
    day: "numeric",
  });

  return (
    <header className="flex h-14 shrink-0 items-center justify-between border-b border-gray-200 bg-white px-4 md:px-6">
      {/* Mobile: brand on the left (the sidebar with the logo is hidden) */}
      <div className="flex items-center gap-2 md:hidden">
        <LogoMark />
        <span className="text-sm font-semibold tracking-tight text-gray-900">Nutrify</span>
      </div>

      <p className="hidden text-sm text-gray-400 md:block">{today}</p>

      {/* Mobile: date + sign out on the right (the sidebar's user section is hidden) */}
      <div className="flex items-center gap-3 md:hidden">
        <span className="text-sm text-gray-400">{todayShort}</span>
        {oidc.isUserLoggedIn && (
          <button
            onClick={() => oidc.logout({ redirectTo: "home" })}
            title="Sign out"
            className="flex h-9 w-9 items-center justify-center rounded-md text-gray-500 transition-colors hover:bg-gray-100 hover:text-gray-700"
          >
            <IconLogout className="h-4 w-4 shrink-0" />
          </button>
        )}
      </div>
    </header>
  );
}
