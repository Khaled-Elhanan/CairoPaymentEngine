import type { InputHTMLAttributes } from "react";
import clsx from "clsx";

type Props = InputHTMLAttributes<HTMLInputElement> & {
  error?: string;
};

export function Input({ className, error, ...props }: Props) {
  return (
    <input
      className={clsx(
        "w-full rounded-xl border bg-white px-3 py-3 text-sm text-gray-800 outline-none transition-all",
        "border-gray-200 focus:ring-2 focus:ring-indigo-400 focus:border-indigo-300",
        error
          ? "border-rose-300 focus:ring-rose-300"
          : "shadow-sm",
        className,
      )}
      {...props}
    />
  );
}
