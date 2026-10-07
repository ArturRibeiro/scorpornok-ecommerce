export default function LoadingState() {
  return (
    <div className="flex justify-center py-16" role="status">
      <div className="w-8 h-8 border-2 border-primary border-t-transparent rounded-full animate-spin" />
      <span className="sr-only">Loading...</span>
    </div>
  );
}
