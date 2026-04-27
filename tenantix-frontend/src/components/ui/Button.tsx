import type { ButtonHTMLAttributes } from "react";
import clsx from "clsx";

type Props = ButtonHTMLAttributes<HTMLButtonElement> & {
  variant?: "primary" | "secondary" | "danger";
};

export function Button({ className, variant = "primary", ...props }: Props) {
  return (
    <button
      className={clsx(
        "w-full rounded-xl px-5 py-3 text-sm font-semibold transition-all duration-200",
        "disabled:cursor-not-allowed disabled:opacity-60",
        "hover:scale-[1.02] hover:shadow-lg",
        variant === "primary" && "bg-gradient-to-r from-indigo-500 to-purple-500 text-white shadow-md",
        variant === "secondary" && "border border-gray-200 bg-white text-gray-700 shadow-sm hover:bg-gray-50",
        variant === "danger" && "bg-gradient-to-r from-rose-500 to-pink-500 text-white shadow-md",
        className,
      )}
      {...props}
    />
  );
}
