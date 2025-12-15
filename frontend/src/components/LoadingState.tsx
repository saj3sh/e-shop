import { Spinner } from "./ui";

interface LoadingStateProps {
  message?: string;
}

export const LoadingState = ({ message = "Loading..." }: LoadingStateProps) => {
  return (
    <div className="flex flex-col items-center justify-center py-12">
      <Spinner size="lg" className="text-blue-600" />
      <p className="mt-4 text-gray-600">{message}</p>
    </div>
  );
};
