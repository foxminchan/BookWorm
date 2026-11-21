"use client";

import useCreatePublisher from "@workspace/api-hooks/catalog/publishers/useCreatePublisher";
import useDeletePublisher from "@workspace/api-hooks/catalog/publishers/useDeletePublisher";
import usePublishers from "@workspace/api-hooks/catalog/publishers/usePublishers";
import useUpdatePublisher from "@workspace/api-hooks/catalog/publishers/useUpdatePublisher";

import { SimpleCatalogListing } from "./simple-catalog-listing";

const breadcrumbs = [
  { label: "Admin", href: "/" },
  { label: "Publishers", isActive: true },
];

const buildCreateRequest = (name: string) => ({ name });
const buildUpdateRequest = (id: string, name: string) => ({
  request: { id, name },
});

export function PublishersListing() {
  return (
    <SimpleCatalogListing
      entityName="Publisher"
      pluralLabel="publishers"
      pageTitle="Publishers Management"
      pageDescription="Manage book publishers"
      dialogTitle="Create Publisher"
      dialogDescription="Enter the publisher's name"
      dialogPlaceholder="Publisher name"
      tableTitle="All Publishers"
      breadcrumbs={breadcrumbs}
      listQuery={usePublishers()}
      createMutation={useCreatePublisher()}
      updateMutation={useUpdatePublisher()}
      deleteMutation={useDeletePublisher()}
      buildCreateRequest={buildCreateRequest}
      buildUpdateRequest={buildUpdateRequest}
    />
  );
}
