"use client";

import { useEffect, useState } from "react";

import { useQuery, useQueryClient } from "@tanstack/react-query";

import dashboardApiClient from "@workspace/api-client/ordering/dashboard";
import { orderingKeys } from "@workspace/api-hooks/keys";
import type { Dashboard } from "@workspace/types/ordering/dashboard";

import { useUserContext } from "@/hooks/useUserContext";

export function useDashboardStats() {
  const queryClient = useQueryClient();
  const { user } = useUserContext();
  const userId = user?.id;
  const [isDisconnected, setIsDisconnected] = useState(false);
  const query = useQuery({
    queryKey: orderingKeys.dashboard(userId),
    queryFn: ({ signal }) => dashboardApiClient.get(signal),
    staleTime: Infinity,
    refetchOnWindowFocus: false,
    enabled: !!userId,
  });
  const hasData = !!query.data;

  useEffect(() => {
    if (!hasData) return;
    return dashboardApiClient.subscribe(
      (snapshot) => {
        queryClient.setQueryData<Dashboard>(
          orderingKeys.dashboard(userId),
          (current) =>
            current &&
            Date.parse(current.updatedAt) > Date.parse(snapshot.updatedAt)
              ? current
              : snapshot,
        );
        setIsDisconnected(false);
      },
      () => setIsDisconnected(true),
    );
  }, [hasData, queryClient, userId]);

  return { ...query, isDisconnected };
}
