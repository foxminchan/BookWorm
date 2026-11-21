export function JsonLd({
  data,
  nonce,
}: Readonly<{ data: object; nonce?: string }>) {
  const jsonString = JSON.stringify(data).replace(/</g, "\\u003c");

  return (
    <script type="application/ld+json" nonce={nonce} suppressHydrationWarning>
      {jsonString}
    </script>
  );
}
