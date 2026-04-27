import clsx from "clsx";

type Props = {
  className?: string;
};

export function Spinner({ className }: Props) {
  return <div className={clsx("h-5 w-5 animate-spin rounded-full border-2 border-slate-300 border-t-blue-600", className)} />;
}
