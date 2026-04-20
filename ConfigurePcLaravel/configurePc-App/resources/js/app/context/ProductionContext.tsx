import { createContext, useContext, useState, useEffect, useCallback, type ReactNode } from 'react';
import type {
  ComputerOrder,
  ProductionFlow,
  ProductionStatus,
  Machine,
  ProductionLog,
  LogLevel,
  ProductionStage,
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
  statusMessage: string;
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

const stageOrder: ProductionStage[] = [
  'website',
  'warehouse-receive',
  'agv-to-assembly',
  'assembly',
  'agv-to-warehouse',
  'warehouse-delivery',
  'delivery'
];

function updateStage(flow: ProductionFlow, stage: ProductionStage, next: ProductionFlow[ProductionStage]): ProductionFlow {
  const current = flow[stage];
  const idx = stageOrder.indexOf(stage);

  if (current === 'completed' && next !== 'completed') return flow;

  if (next === 'in-progress' && idx > 0) {
    const prev = stageOrder[idx -1];
    if (flow[prev] !== 'completed') return flow;
  }

  if (current === next) return flow;
  return { ...flow, [stage]: next };
}

// ─── Provider ─────────────────────────────────────────────────────────────────

export function ProductionProvider({ children }: { children: ReactNode }) {
  const [isAuthenticated, setIsAuthenticated]     = useState(false);
  const [productionStatus, setProductionStatus]   = useState<ProductionStatus>('running');
  const [currentOrder, setCurrentOrder]           = useState<ComputerOrder | null>(null);
  const [queue, setQueue]                         = useState<ComputerOrder[]>([]);
  const [machines, setMachines]                   = useState<Machine[]>(initialMachines);
  const [logs, setLogs]                           = useState<ProductionLog[]>([]);
  const [productionFlow, setProductionFlow]       = useState<ProductionFlow>({ ...emptyFlow });
  const [statusMessage, setStatusMessage] = useState('');

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
  
  useEffect(() => {
    const timers: number[] = [];
    const setSafeTimeout = (fn: () => void, ms: number) => {
      const id = window.setTimeout(fn, ms);
      timers.push(id);
      return id;
    };

    const source = new EventSource('http://localhost:5027/ProductionSystem/Events');

    source.onmessage = (event) => {
      const e = JSON.parse(event.data) as {
        DateAndTime: string;
        Description: string;
        Source: string;
        Type: string;
        Level: string;
      };

      const levelMap: Record<string, LogLevel> = {
        low: 'info',
        medium: 'warning',
        high: 'error',
      };

      addLog(
        levelMap[e.Level?.toLowerCase()] ?? 'info',
        e.Source ?? 'System',
        e.Type ?? 'Event',
        e.Description ?? ''
      );

      const src = (e.Source ?? '').toLowerCase();
      const desc = (e.Description ?? '').trim();

      setMachines(prev =>
        prev.map(m => {
          const matchesType =
            (src.includes('warehouse') && m.type === 'warehouse') ||
            (src.includes('agv') && m.type === 'agv') ||
            (src.includes('assembly') && m.type === 'assembly');

          return matchesType ? { ...m, state: 'working', currentTask: desc} : m;
        })
      );

      if ((e.Type ?? '').toLowerCase() === 'step-status') {
        const [rawStage, rawState, ...messageParts] = desc.split('|');
        const stage = (rawStage ?? '').trim() as ProductionStage;
        const stateText = (rawState ?? '').trim().toLowerCase();
        const message = messageParts.join('|').trim();

        if (!stageOrder.includes(stage))
          return;

        let nextState: ProductionFlow[ProductionStage] | null = null;
        if (stateText === 'in-progress') nextState = 'in-progress';
        else if (stateText === 'completed') nextState = 'completed';
        else if (stateText === 'pending') nextState = 'pending';
        else if (stateText === 'error') nextState = 'error';

        if (!nextState)
          return;

        if (stage === 'website' && nextState === 'in-progress') {
          setCurrentOrder(prev =>
            prev ?? {
              id: `ORD-${Date.now()}`,
              cpu: '',
              gpu: '',
              ram: '',
              storage: '',
              motherboard: '',
              powerSupply: '',
              case: '',
              createdAt: new Date(),
            }
          );
        }

        setProductionFlow(prev => updateStage(prev, stage, nextState));
        setStatusMessage(message || `${stage} ${nextState}`);

        if (stage === 'delivery' && nextState === 'completed') {
          setSafeTimeout(() => {
            setProductionFlow({ ...emptyFlow });
            setCurrentOrder(null);
            setStatusMessage('Order complete.');
          }, 3000);
        }
      }
    };

    source.onerror = () => {
      setMachines(prev => prev.map(m => ({ ...m, status: 'disconnected' as const })));
    };

    return () => {
      source.close();
      timers.forEach(t => clearTimeout(t));
    };
  }, [addLog]);

  return (
    <ProductionContext.Provider value={{
      isAuthenticated, login, logout,
      productionStatus, currentOrder, queue, productionFlow,
      machines, logs,
      stopProduction, resetProduction, resumeProduction, statusMessage,
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
