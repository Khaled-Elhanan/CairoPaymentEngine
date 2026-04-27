import type { ReactNode } from "react";

type Props = {
  label: string;
  htmlFor: string;
  error?: string;
  children: ReactNode;
};

export function FormField({ label, htmlFor, error, children }: Props) {
  return (
    <div className="space-y-1.5">
      <label htmlFor={htmlFor} className="block text-sm font-medium text-gray-500">
        {label}
      </label>
      {children}
      {error ? <p className="text-xs font-medium text-rose-500">{error}</p> : null}
    </div>
  );
}
