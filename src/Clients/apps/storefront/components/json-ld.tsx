export function JsonLd({ data }: Readonly<{ data: object }>) {
  const jsonString = JSON.stringify(data);

  return (
    <script type="application/ld+json" suppressHydrationWarning>
      {jsonString}
    </script>
  );
}
