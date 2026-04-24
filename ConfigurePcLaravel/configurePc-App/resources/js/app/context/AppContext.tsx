import { createContext, useContext, useEffect, useState, type ReactNode } from 'react';
import { PCComponent, mapApiComponent } from '../data/components';
import type { ProductionStage, StageState } from '../types/production';

export type OrderInfo = {
  name: string;
  email: string;
  address: string;
};

export const ORDER_STATUSES = [
  { id: 0, label: 'Order Placed',                 sublabel: 'Received online' },
  { id: 1, label: 'Components Ready',              sublabel: 'Warehouse prepared' },
  { id: 2, label: 'Transporting Components',       sublabel: 'Parts en route to assembly' },
  { id: 3, label: 'Building Computer',             sublabel: 'Assembly in progress' },
  { id: 4, label: 'Transporting Computer',         sublabel: 'Shipped to delivery hub' },
  { id: 5, label: 'Ready for Delivery',            sublabel: 'Out for delivery' },
];

type AppContextType = {
  // Configuration
  selectedComponents: Record<string, PCComponent | null>;
  selectComponent: (component: PCComponent) => void;
  deselectComponent: (categoryId: string) => void;
  clearConfiguration: () => void;
  components: PCComponent[];

  // Order
  orderInfo: OrderInfo | null;
  currentOrderId: number | null;
  orderStatus: number;
  hasActiveOrder: boolean;
  placeOrder: (info: OrderInfo) => Promise<void>;
  cancelOrder: () => void;
  advanceOrderStatus: () => void; // demo helper
};

const AppContext = createContext<AppContextType | null>(null);

const emptySelection: Record<string, PCComponent | null> = {
  cpu: null,
  motherboard: null,
  ram: null,
  gpu: null,
  storage: null,
  psu: null,
  case: null,
  cooling: null,
};

export function AppProvider({ children }: { children: ReactNode }) {
  const [selectedComponents, setSelectedComponents] = useState<Record<string, PCComponent | null>>(
    { ...emptySelection }
  );
  const [orderInfo, setOrderInfo] = useState<OrderInfo | null>(null);
  const [currentOrderId, setCurrentOrderId] = useState<number | null>(null);
  const [orderStatus, setOrderStatus] = useState<number>(0);
  const [hasActiveOrder, setHasActiveOrder] = useState(false);

  const selectComponent = (component: PCComponent) => {
    setSelectedComponents(prev => ({ ...prev, [component.categoryId]: component }));
  };
  
  const deselectComponent = (categoryId: string) => {
    setSelectedComponents(prev => ({ ...prev, [categoryId]: null }));
  };

  const clearConfiguration = () => {
    setSelectedComponents({ ...emptySelection });
  };

  const placeOrder = async (info: OrderInfo) => {
    const trayIds = Object.values(selectedComponents)
      .filter(Boolean)
      .map(c => c!.trayId);

    const orderId = Math.floor(Date.now() / 1000);
    console.log('Placing order:', { orderId, trayIds });

    await fetch('/api/orders', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ id: orderId, trayIds }),
    });

    setOrderInfo(info);
    setCurrentOrderId(orderId);
    setOrderStatus(0);
    setHasActiveOrder(true);
  };

  const cancelOrder = () => {
    setOrderInfo(null);
    setCurrentOrderId(null);
    setOrderStatus(0);
    setHasActiveOrder(false);
  };

  const advanceOrderStatus = () => {
    setOrderStatus(prev => Math.min(prev + 1, ORDER_STATUSES.length - 1));
  };

  const [components, setComponents] = useState<PCComponent[]>([]);

  useEffect(() => {
    fetch('/api/components')
      .then(r => r.json())
      .then(data => setComponents(data.map(mapApiComponent)));
  }, []);

  useEffect(() => {
    if (!hasActiveOrder || currentOrderId == null) return;

    const stageToIndex: Record<ProductionStage, number> = {
      website: 0,
      'warehouse-receive': 1,
      'agv-to-assembly': 2,
      assembly: 3,
      'agv-to-warehouse': 4,
      'warehouse-delivery': 4,
      delivery: 5,
    };

    const syncStatus = async () => {
      const response = await fetch(`/api/production/order-status/${currentOrderId}`);
      if (!response.ok) return;

      const snapshot = await response.json() as {
        stage: ProductionStage;
        state: StageState;
      };

      const index = stageToIndex[snapshot.stage] ?? 0;
      const isCompleted = snapshot.state === 'completed';
      const nextIndex = Math.max(0, Math.min(index + (isCompleted ? 1 : 0), ORDER_STATUSES.length - 1));
      setOrderStatus(nextIndex);

      if (snapshot.stage === 'delivery' && isCompleted) {
        setHasActiveOrder(false);
      }
    };

    void syncStatus();
    const intervalId = window.setInterval(() => {
      void syncStatus();
    }, 2000);

    return () => {
      clearInterval(intervalId);
    };
  }, [currentOrderId, hasActiveOrder]);

  return (
    <AppContext.Provider value={{
      selectedComponents,
      selectComponent,
      deselectComponent,
      clearConfiguration,
      components,
      orderInfo,
      currentOrderId,
      orderStatus,
      hasActiveOrder,
      placeOrder,
      cancelOrder,
      advanceOrderStatus,
    }}>
      {children}
    </AppContext.Provider>
  );
}



export function useApp() {
  const ctx = useContext(AppContext);
  if (!ctx) throw new Error('useApp must be used within AppProvider');
  return ctx;
}
