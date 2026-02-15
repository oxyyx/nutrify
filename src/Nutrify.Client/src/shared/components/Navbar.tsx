export function Navbar() {
  const today = new Date().toLocaleDateString(undefined, {
    weekday: "long",
    month: "long",
    day: "numeric",
  });

  return (
    <header className="flex h-14 shrink-0 items-center border-b border-gray-200 bg-white px-6">
      <p className="text-sm text-gray-400">{today}</p>
    </header>
  );
}
