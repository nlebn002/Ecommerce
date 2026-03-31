export interface SummaryItem {
  label: string;
  value: string;
}

export interface RequestHistoryEntry {
  id: string;
  label: string;
  startedAt: string;
  ok: boolean;
}

export interface ResponseState {
  label: string;
  status: '' | 'ok' | 'error' | 'working';
  payload: unknown;
}

export interface BasketDraft {
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
}

export interface ProductSeed {
  name: string;
  price: number;
}
