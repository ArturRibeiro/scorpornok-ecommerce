
import React, { createContext, useContext, useSyncExternalStore } from "react";

interface CartItem {
  id: number;
  name: string;
  price: number;
  image: string;
  quantity: number;
}

interface CartContextProps {
  cart: CartItem[];
  addToCart: (item: CartItem) => void;
  removeFromCart: (name: number) => void;
  clearCart: () => void;
  updateQuantity: (id: number, quantity: number) => void;
}

const CartContext = createContext<CartContextProps | undefined>(undefined);

// O localStorage é a fonte do carrinho, lida via useSyncExternalStore.
const CART_KEY = "cart";
const EMPTY_CART: CartItem[] = [];
const listeners = new Set<() => void>();
let cachedRaw: string | null = null;
let cachedCart: CartItem[] = EMPTY_CART;

// O snapshot precisa manter a mesma referência enquanto o conteúdo
// salvo não mudar, senão o React re-renderiza em loop.
const getCartSnapshot = () => {
  const raw = localStorage.getItem(CART_KEY);
  if (raw !== cachedRaw) {
    cachedRaw = raw;
    cachedCart = raw ? JSON.parse(raw) : EMPTY_CART;
  }
  return cachedCart;
};

const subscribeToCart = (listener: () => void) => {
  listeners.add(listener);
  // O evento "storage" sincroniza o carrinho entre abas.
  window.addEventListener("storage", listener);
  return () => {
    listeners.delete(listener);
    window.removeEventListener("storage", listener);
  };
};

const setCart = (update: (prevCart: CartItem[]) => CartItem[]) => {
  localStorage.setItem(CART_KEY, JSON.stringify(update(getCartSnapshot())));
  listeners.forEach((listener) => listener());
};

export const CartProvider = ({ children }: { children: React.ReactNode }) => {
  const cart = useSyncExternalStore(subscribeToCart, getCartSnapshot);

  const addToCart = (item: CartItem) => {
    setCart((prevCart) => {
      const existingItem = prevCart.find((cartItem) => cartItem.id === item.id);

      if (existingItem) {
        return prevCart.map((cartItem) =>
          cartItem.id === item.id
            ? { ...cartItem, quantity: cartItem.quantity + 1 }
            : cartItem
        );
      }

      return [...prevCart, { ...item, quantity: 1 }];
    });
  };

  const removeFromCart = (id: number) => {
    setCart((prevCart) => prevCart.filter((item) => item.id !== id));
  };

  const clearCart = () => {
    localStorage.removeItem(CART_KEY);
    listeners.forEach((listener) => listener());
  };

  const updateQuantity = (id: number, quantity: number) => {
    setCart((prevCart) =>
      prevCart.map((item) =>
        item.id === id ? { ...item, quantity: Math.max(1, quantity) } : item
      )
    );
  };

  return (
    <CartContext.Provider
      value={{ cart, addToCart, removeFromCart, clearCart, updateQuantity }}
    >
      {children}
    </CartContext.Provider>
  );
};

export const useCart = () => {
  const context = useContext(CartContext);
  if (!context) {
    throw new Error("useCart must be used within a CartProvider");
  }
  return context;
};
