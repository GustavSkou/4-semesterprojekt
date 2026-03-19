import { createContext, useContext, useState, useEffect, useCallback, type ReactNode } from 'react';
import type {
  ComputerOrder,
  ProductionFlow,
  ProductionStatus,
  Machine,
  ProductionLog,
  LogLevel,
} from '../types/production';

interface ProductionContextType {
  isAuthenticated: boolean;
  login: (email: string, password: string) => boolean;
  logout: () => void;
  productionStatus: ProductionStatus;
  currentOrder: ComputerOrder | null;
  queue: ComputerOrder[];
  productionFlow: ProductionFlow;
  machines: Machine[];
  logs: ProductionLog[];
  stopProduction: () => void;
  resetProduction: () => void;
  resumeProduction: () => void;
}

const ProductionContext = createContext<ProductionContextType | undefined>(undefined);

// ─── Mock data ────────────────────────────────────────────────────────────────

const generateMockOrders = (): ComputerOrder[] => {
  const cpus = ['Intel i9-13900K', 'AMD Ryzen 9 7950X', 'Intel i7-13700K', 'AMD Ryzen 7 7800X3D'];
  const gpus = ['NVIDIA RTX 4090', 'NVIDIA RTX 4080', 'AMD RX 7900 XTX', 'NVIDIA RTX 4070 Ti'];
  const rams = ['32GB DDR5-6000', '64GB DDR5-5600', '32GB DDR4-3600', '128GB DDR5-6400'];
  const storages = ['2TB NVMe SSD', '4TB NVMe SSD', '1TB NVMe SSD + 4TB HDD', '8TB NVMe SSD'];
  const motherboards = ['ASUS ROG Maximus Z790', 'MSI MAG B650 Tomahawk', 'Gigabyte X670E Aorus Master', 'ASRock Z790 Taichi'];
  const powerSupplies = ['850W 80+ Gold', '1000W 80+ Platinum', '750W 80+ Gold', '1200W 80+ Titanium'];
  const cases = ['Lian Li O11 Dynamic', 'Fractal Design Meshify 2', 'NZXT H710i', 'Corsair 5000D'];

  return Array.from({ length: 8 }, (_, i) => ({
    id: `ORD-${String(i + 1001).padStart(6, '0')}`,
    cpu: cpus[Math.floor(Math.random() * cpus.length)],
    gpu: gpus[Math.floor(Math.random() * gpus.length)],
    ram: rams[Math.floor(Math.random() * rams.length)],
    storage: storages[Math.floor(Math.random() * storages.length)],
    motherboard: motherboards[Math.floor(Math.random() * motherboards.length)],
    powerSupply: powerSupplies[Math.floor(Math.random() * powerSupplies.length)],
    case: cases[Math.floor(Math.random() * cases.length)],
    createdAt: new Date(Date.now() - Math.random() * 3600000),
  }));
};

const initialMachines: Machine[] = [
  { id: 'WH-001',  name: 'Warehouse System',        type: 'warehouse', status: 'connected', state: 'idle', currentTask: 'Standby' },
  { id: 'AGV-001', name: 'AGV Transport Unit 1',     type: 'agv',       status: 'connected', state: 'idle', currentTask: 'Standby' },
  { id: 'AGV-002', name: 'AGV Transport Unit 2',     type: 'agv',       status: 'connected', state: 'idle', currentTask: 'Standby' },
  { id: 'ASM-001', name: 'Assembly Station Alpha',   type: 'assembly',  status: 'connected', state: 'idle', currentTask: 'Standby' },
];

const emptyFlow: ProductionFlow = {
  website: 'pending',
  'warehouse-receive': 'pending',
  'agv-to-assembly': 'pending',
  assembly: 'pending',
  'agv-to-warehouse': 'pending',
  'warehouse-delivery': 'pending',
  delivery: 'pending',
};

// ─── Provider ─────────────────────────────────────────────────────────────────

export function ProductionProvider({ children }: { children: ReactNode }) {
  const [isAuthenticated, setIsAuthenticated]     = useState(false);
  const [productionStatus, setProductionStatus]   = useState<ProductionStatus>('running');
  const [currentOrder, setCurrentOrder]           = useState<ComputerOrder | null>(null);
  const [queue, setQueue]                         = useState<ComputerOrder[]>([]);
  const [machines, setMachines]                   = useState<Machine[]>(initialMachines);
  const [logs, setLogs]                           = useState<ProductionLog[]>([]);
  const [productionFlow, setProductionFlow]       = useState<ProductionFlow>({ ...emptyFlow });
  const [isReadyForDelivery, setIsReadyForDelivery] = useState(false);

  const addLog = useCallback((level: LogLevel, source: string, type: string, description: string) => {
    const newLog: ProductionLog = {
      id: `LOG-${Date.now()}-${Math.random().toString(36).substring(2, 9)}`,
      timestamp: new Date(),
      level, source, type, description,
    };
    setLogs(prev => [newLog, ...prev].slice(0, 100));
  }, []);

  // ── Auth ──────────────────────────────────────────────────────────────────

  const login = useCallback((email: string, password: string): boolean => {

     const VALID_EMAIL = 'operator@example.com';
  const VALID_PASSWORD = '1234';

    if (email === VALID_EMAIL && password === VALID_PASSWORD) {
      setIsAuthenticated(true);
      addLog('success', 'Auth', 'Login', `Operator logged in: ${email}`);
      return true;
    }
    return false;
  }, [addLog]);

  const logout = useCallback(() => {
    setIsAuthenticated(false);
    addLog('info', 'Auth', 'Logout', 'Operator logged out');
  }, [addLog]);

  // ── Production controls ───────────────────────────────────────────────────

  const stopProduction = useCallback(() => {
    setProductionStatus('stopped');
    setMachines(prev => prev.map(m => ({ ...m, state: 'idle', currentTask: 'Production stopped' })));
    addLog('warning', 'Control', 'Stop', 'Production stopped by operator');
  }, [addLog]);

  const resetProduction = useCallback(() => {
    setProductionStatus('reset');
    setCurrentOrder(null);
    setProductionFlow({ ...emptyFlow });
    setMachines(prev => prev.map(m => ({ ...m, state: 'idle', currentTask: 'Standby' })));
    setIsReadyForDelivery(false);
    addLog('info', 'Control', 'Reset', 'Production system reset');
  }, [addLog]);

  const resumeProduction = useCallback(() => {
    setProductionStatus('running');
    addLog('success', 'Control', 'Resume', 'Production resumed');
  }, [addLog]);

  // ── Queue init ────────────────────────────────────────────────────────────

  useEffect(() => {
    const initialQueue = generateMockOrders();
    setQueue(initialQueue);
    addLog('info', 'System', 'Init', 'Production system initialized');
  }, [addLog]);

  // ── Production simulation ─────────────────────────────────────────────────

  useEffect(() => {
    if (productionStatus !== 'running') return;
    if (!currentOrder && queue.length === 0) return;

    if (!currentOrder && queue.length > 0) {
      const [nextOrder, ...remainingQueue] = queue;
      setCurrentOrder(nextOrder);
      setQueue(remainingQueue);
      setProductionFlow({ ...emptyFlow, website: 'completed', 'warehouse-receive': 'in-progress' });
      setIsReadyForDelivery(false);
      addLog('info', 'Queue', 'Order Started', `Processing order ${nextOrder.id}`);
      setMachines(prev => prev.map(m =>
        m.type === 'warehouse'
          ? { ...m, state: 'working', currentTask: `Receiving parts for ${nextOrder.id}` }
          : m
      ));
      return;
    }

    if (!currentOrder) return;

    const stages = [
      'warehouse-receive', 'agv-to-assembly', 'assembly',
      'agv-to-warehouse', 'warehouse-delivery', 'delivery',
    ] as const;

    const currentStageIndex = stages.findIndex(s => productionFlow[s] === 'in-progress');
    if (currentStageIndex === -1) return;

    const timer = setTimeout(() => {
      const stage = stages[currentStageIndex];

      if (stage === 'warehouse-receive') {
        setProductionFlow(prev => ({ ...prev, 'warehouse-receive': 'completed', 'agv-to-assembly': 'in-progress' }));
        setMachines(prev => prev.map(m => {
          if (m.type === 'warehouse') return { ...m, state: 'idle', currentTask: 'Standby' };
          if (m.id === 'AGV-001') return { ...m, state: 'working', currentTask: `Transporting ${currentOrder.id} to assembly` };
          return m;
        }));
        addLog('info', 'AGV-001', 'Transport', 'Parts loaded, moving to assembly station');

      } else if (stage === 'agv-to-assembly') {
        setProductionFlow(prev => ({ ...prev, 'agv-to-assembly': 'completed', assembly: 'in-progress' }));
        setMachines(prev => prev.map(m => {
          if (m.id === 'AGV-001') return { ...m, state: 'idle', currentTask: 'Standby' };
          if (m.type === 'assembly') return { ...m, state: 'working', currentTask: `Assembling ${currentOrder.id}` };
          return m;
        }));
        addLog('info', 'ASM-001', 'Assembly', `Starting assembly of ${currentOrder.id}`);

      } else if (stage === 'assembly') {
        setProductionFlow(prev => ({ ...prev, assembly: 'completed', 'agv-to-warehouse': 'in-progress' }));
        setMachines(prev => prev.map(m => {
          if (m.type === 'assembly') return { ...m, state: 'idle', currentTask: 'Standby' };
          if (m.id === 'AGV-002') return { ...m, state: 'working', currentTask: `Transporting ${currentOrder.id} to warehouse` };
          return m;
        }));
        addLog('success', 'ASM-001', 'Assembly', `Assembly completed for ${currentOrder.id}`);

      } else if (stage === 'agv-to-warehouse') {
        setProductionFlow(prev => ({ ...prev, 'agv-to-warehouse': 'completed', 'warehouse-delivery': 'in-progress' }));
        setMachines(prev => prev.map(m => {
          if (m.id === 'AGV-002') return { ...m, state: 'idle', currentTask: 'Standby' };
          if (m.type === 'warehouse') return { ...m, state: 'working', currentTask: `Preparing ${currentOrder.id} for delivery` };
          return m;
        }));
        addLog('info', 'WH-001', 'Warehouse', `Computer ${currentOrder.id} ready for delivery prep`);

      } else if (stage === 'warehouse-delivery') {
        setProductionFlow(prev => ({ ...prev, 'warehouse-delivery': 'completed', delivery: 'in-progress' }));
        setIsReadyForDelivery(true);
        setMachines(prev => prev.map(m =>
          m.type === 'warehouse' ? { ...m, state: 'idle', currentTask: 'Standby' } : m
        ));
        addLog('success', 'WH-001', 'Delivery', `${currentOrder.id} ready for delivery`);
      }
    }, 3000);

    return () => clearTimeout(timer);
  }, [productionStatus, currentOrder, queue, productionFlow, addLog]);

  // ── Delivery completion ────────────────────────────────────────────────────

  useEffect(() => {
    if (!isReadyForDelivery || !currentOrder) return;

    const timer = setTimeout(() => {
      addLog('info', 'Delivery', 'Dispatch', `${currentOrder.id} out for delivery`);
      setCurrentOrder(null);
      setProductionFlow({ ...emptyFlow });
      setIsReadyForDelivery(false);
    }, 5000);

    return () => clearTimeout(timer);
  }, [isReadyForDelivery, currentOrder, addLog]);

  return (
    <ProductionContext.Provider value={{
      isAuthenticated, login, logout,
      productionStatus, currentOrder, queue, productionFlow,
      machines, logs,
      stopProduction, resetProduction, resumeProduction,
    }}>
      {children}
    </ProductionContext.Provider>
  );
}

export function useProduction() {
  const ctx = useContext(ProductionContext);
  if (!ctx) throw new Error('useProduction must be used within ProductionProvider');
  return ctx;
}
