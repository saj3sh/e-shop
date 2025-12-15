import { create } from "zustand";

export interface CartItem {
  productId: string;
  name: string;
  price: number;
  quantity: number;
}

interface CartState {
  items: CartItem[];
  currentUserId: string | null;
  addItem: (item: Omit<CartItem, "quantity"> & { quantity?: number }) => void;
  updateQuantity: (productId: string, quantity: number) => void;
  removeItem: (productId: string) => void;
  clearCart: () => void;
  getTotalItems: () => number;
  getTotalPrice: () => number;
  switchUser: (userId: string | null) => void;
}

const getStorageKey = (userId: string | null) =>
  userId ? `cart-storage-${userId}` : "cart-storage-guest";

const loadCart = (userId: string | null): CartItem[] => {
  try {
    const stored = localStorage.getItem(getStorageKey(userId));
    return stored ? JSON.parse(stored) : [];
  } catch (e) {
    console.error("Error loading cart:", e);
    return [];
  }
};

const saveCart = (userId: string | null, items: CartItem[]) => {
  try {
    localStorage.setItem(getStorageKey(userId), JSON.stringify(items));
  } catch (e) {
    console.error("Error saving cart:", e);
  }
};

export const useCartStore = create<CartState>((set, get) => ({
  items: loadCart(null), // starting with guest cart
  currentUserId: null,

  addItem: (item) => {
    const { items, currentUserId } = get();
    const existing = items.find((i) => i.productId === item.productId);

    const newItems = existing
      ? items.map((i) =>
          i.productId === item.productId
            ? { ...i, quantity: i.quantity + (item.quantity || 1) }
            : i
        )
      : [...items, { ...item, quantity: item.quantity || 1 }];

    set({ items: newItems });
    saveCart(currentUserId, newItems);
  },

  updateQuantity: (productId, quantity) => {
    if (quantity <= 0) {
      get().removeItem(productId);
      return;
    }

    const { items, currentUserId } = get();
    const newItems = items.map((i) =>
      i.productId === productId ? { ...i, quantity } : i
    );

    set({ items: newItems });
    saveCart(currentUserId, newItems);
  },

  removeItem: (productId) => {
    const { items, currentUserId } = get();
    const newItems = items.filter((i) => i.productId !== productId);

    set({ items: newItems });
    saveCart(currentUserId, newItems);
  },

  clearCart: () => {
    const { currentUserId } = get();
    set({ items: [] });
    saveCart(currentUserId, []);
  },

  getTotalItems: () => {
    return get().items.reduce((sum, item) => sum + item.quantity, 0);
  },

  getTotalPrice: () => {
    return get().items.reduce(
      (sum, item) => sum + item.price * item.quantity,
      0
    );
  },

  switchUser: (userId) => {
    const items = loadCart(userId);
    set({ items, currentUserId: userId });
  },
}));
