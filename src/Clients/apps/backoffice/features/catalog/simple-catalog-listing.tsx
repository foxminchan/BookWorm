"use client";

import type { UseMutationResult, UseQueryResult } from "@tanstack/react-query";

import { Button } from "@workspace/ui/components/button";

import { PageContainer } from "@/components/page-container";
import { SimpleDialog } from "@/components/simple-dialog";
import { SimpleTable } from "@/components/simple-table";
import { useCrudPage } from "@/hooks/useCrudPage";

type BreadcrumbItem = {
  label: string;
  href?: string;
  isActive?: boolean;
};

type NamedEntity = {
  id: string;
  name: string | null;
};

type SimpleCatalogListingProps<
  TItem extends NamedEntity,
  TCreateResult,
  TUpdateResult,
  TCreateRequest,
  TUpdateRequest,
> = Readonly<{
  entityName: string;
  pluralLabel: string;
  pageTitle: string;
  pageDescription: string;
  dialogTitle: string;
  dialogDescription: string;
  dialogPlaceholder: string;
  tableTitle: string;
  breadcrumbs: BreadcrumbItem[];
  listQuery: UseQueryResult<TItem[]>;
  createMutation: UseMutationResult<TCreateResult, Error, TCreateRequest>;
  updateMutation: UseMutationResult<TUpdateResult, Error, TUpdateRequest>;
  deleteMutation: UseMutationResult<void, Error, string>;
  buildCreateRequest: (name: string) => TCreateRequest;
  buildUpdateRequest: (id: string, name: string) => TUpdateRequest;
}>;

/**
 * Shared listing view for simple named catalog entities (authors, categories, publishers).
 */
export function SimpleCatalogListing<
  TItem extends NamedEntity,
  TCreateResult,
  TUpdateResult,
  TCreateRequest,
  TUpdateRequest,
>({
  entityName,
  pluralLabel,
  pageTitle,
  pageDescription,
  dialogTitle,
  dialogDescription,
  dialogPlaceholder,
  tableTitle,
  breadcrumbs,
  listQuery,
  createMutation,
  updateMutation,
  deleteMutation,
  buildCreateRequest,
  buildUpdateRequest,
}: SimpleCatalogListingProps<
  TItem,
  TCreateResult,
  TUpdateResult,
  TCreateRequest,
  TUpdateRequest
>) {
  const {
    items,
    isLoading,
    isDialogOpen,
    setIsDialogOpen,
    isSubmitting,
    isCreatePending,
    handleCreate,
    handleUpdate,
    handleDelete,
  } = useCrudPage({
    entityName,
    listQuery,
    createMutation,
    updateMutation,
    deleteMutation,
    buildCreateRequest,
    buildUpdateRequest,
  });

  return (
    <PageContainer
      title={pageTitle}
      description={pageDescription}
      breadcrumbs={breadcrumbs}
      action={
        <Button onClick={() => setIsDialogOpen(true)}>{dialogTitle}</Button>
      }
    >
      <SimpleDialog
        open={isDialogOpen}
        onOpenChange={setIsDialogOpen}
        title={dialogTitle}
        description={dialogDescription}
        placeholder={dialogPlaceholder}
        onSubmit={handleCreate}
        isLoading={isCreatePending}
      />

      <SimpleTable
        title={tableTitle}
        description={`Total: ${items.length} ${pluralLabel}`}
        items={items}
        isLoading={isLoading}
        onUpdate={handleUpdate}
        onDelete={handleDelete}
        isSubmitting={isSubmitting}
      />
    </PageContainer>
  );
}
