<script lang="ts">
  import { gql, queryStore } from '@urql/svelte';
  import { onMount } from 'svelte';
  import { paymentsUrql } from '$lib/stores/urql';
  import type { Trip } from '$lib/types';
  import RecentTransactions from '../../lib/recent-transactions.svelte';

  let tripId: string | undefined;
  let tripTitle: string | undefined;

  onMount(() => {
      tripId = new URLSearchParams(window.location.search)?.get('id') ?? '';
  });

  $: tripsQuery = queryStore({
    client: $paymentsUrql,
    query: gql`
      query {
        transactionTags {
          id
          title
        }
      }
    `,
  });

  $: { tripTitle = tripId && $tripsQuery.data ? $tripsQuery.data.transactionTags.find((tag: Trip) => tag.id === tripId)?.title : ''; }

  $: transactionsForTripQuery = tripId && tripTitle ? queryStore({
    client: $paymentsUrql,
    query: gql`
      query TransactionsForTrip($tags: [String!]!) {
        transactions(filters: { tags: $tags }, first: 50) {
          nodes {
            id
            amount
            merchant
            createdAt
            paidAt
            updatedAt
            tags
          }
        }
      }
    `,
    variables: {
      tags: [tripTitle]
    }
  }) : null;

</script>

<section>
  <h1>{tripTitle}</h1>

   <RecentTransactions
      title=""
      viewType={'all'}
      showAllTransactions={true}
      showGraph={false}
      showViewAllCTA={false}
      transactions={$transactionsForTripQuery?.data?.transactions?.nodes ?? []}
    />
</section>

<style>
  section {
    padding: 1rem;
  }
</style>
