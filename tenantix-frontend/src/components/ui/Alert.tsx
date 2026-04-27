type Props = {
  message: string;
};

export function Alert({ message }: Props) {
  return <div className="rounded-lg border border-red-200 bg-red-50 p-3 text-sm text-red-700">{message}</div>;
}
