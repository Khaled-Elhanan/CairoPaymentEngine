import { Link } from "react-router-dom";

export function NotFoundPage() {
  return (
    <div className="flex min-h-[60vh] flex-col items-center justify-center gap-4">
      <h2 className="text-2xl font-semibold text-slate-900">Page not found</h2>
      <Link className="text-blue-700 hover:underline" to="/">
        Back to dashboard
      </Link>
    </div>
  );
}
