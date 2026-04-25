import { createContext, useContext, useState, useEffect, useCallback, type ReactNode } from 'react';
import type {
  ProductionFlow,
  ProductionStatus,
  Machine,
  ProductionLog,
  LogLevel,
  ProductionStage,
  QueueOrder,
  StageState,
} from '../types/production';

interface ProductionContextType {
  isAuthenticated: boolean;
  login: (email: string, password: string) => boolean;
  logout: () => void;
  productionStatus: ProductionStatus;
  currentOrder: QueueOrder | null;
  queue: QueueOrder[];
  productionFlow: ProductionFlow;
  machines: Machine[];
  logs: ProductionLog[];
  stopProduction: () => void;
  resetProduction: () => void;
  resumeProduction: () => void;
  statusMessage: string;
}

const ProductionContext = createContext<ProductionContextType | undefined>(undefined);

const initialMachines: Machine[] = [
  { id: 'warehouse1', name: 'Warehouse 1', type: 'warehouse', status: 'disconnected', state: 'offline', currentTask: 'Waiting for connection' },
  { id: 'warehouse2', name: 'Warehouse 2', type: 'warehouse', status: 'disconnected', state: 'offline', currentTask: 'Waiting for connection' },
  { id: 'warehouse3', name: 'Warehouse 3', type: 'warehouse', status: 'disconnected', state: 'offline', currentTask: 'Waiting for connection' },
  { id: 'warehouse4', name: 'Warehouse 4', type: 'warehouse', status: 'disconnected', state: 'offline', currentTask: 'Waiting for connection' },
  { id: 'warehouse5', name: 'Warehouse 5', type: 'warehouse', status: 'disconnected', state: 'offline', currentTask: 'Waiting for connection' },
  { id: 'agv', name: 'AGV Transport', type: 'agv', status: 'disconnected', state: 'offline', currentTask: 'Waiting for connection' },
  { id: 'assembly', name: 'Assembly Station', type: 'assembly', status: 'disconnected', state: 'offline', currentTask: 'Waiting for connection' },
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

function flowFromSnapshot(stage: ProductionStage, state: StageState): ProductionFlow {
  const flow: ProductionFlow = { ...emptyFlow };
  const stageIndex = stageOrder.indexOf(stage);

  if (stageIndex < 0) return flow;

  for (let i = 0; i < stageIndex; i += 1) {
    flow[stageOrder[i]] = 'completed';
  }

  flow[stage] = state;
  return flow;
}

const OPERATOR_AUTH_STORAGE_KEY = 'operator:isAuthenticated';

function readPersistedOperatorAuth(): boolean {
  if (typeof window === 'undefined') return false;
  return window.localStorage.getItem(OPERATOR_AUTH_STORAGE_KEY) === 'true';
}

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
  const [isAuthenticated, setIsAuthenticated]     = useState<boolean>(() => readPersistedOperatorAuth());
  const [productionStatus, setProductionStatus]   = useState<ProductionStatus>('running');
  const [currentOrder, setCurrentOrder]           = useState<QueueOrder | null>(null);
  const [queue, setQueue]                         = useState<QueueOrder[]>([]);
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

  const normalizeLogLevel = useCallback((raw: string | null | undefined): LogLevel => {
    const value = (raw ?? '').toLowerCase();
    if (value === 'high' || value === 'error') return 'error';
    if (value === 'medium' || value === 'warning') return 'warning';
    if (value === 'success' || value === 'ok') return 'success';
    return 'info';
  }, []);

  const fetchMachines = useCallback(async () => {
    const response = await fetch('/api/production/machines');
    if (!response.ok) return;

    const payload = await response.json() as {
      machines?: Array<{
        id: string;
        name: string;
        type: string;
        connectionStatus: string;
        state: string;
        currentTask: string;
      }>;
    };

    const nextMachines = (payload.machines ?? []).map((m) => {
      const type = (m.type === 'warehouse' || m.type === 'agv' || m.type === 'assembly')
        ? m.type
        : 'warehouse';

      const state = (m.state === 'idle' || m.state === 'working' || m.state === 'error' || m.state === 'offline')
        ? m.state
        : 'idle';

      return {
        id: m.id,
        name: m.name,
        type,
        status: m.connectionStatus === 'connected' ? 'connected' : 'disconnected',
        state,
        currentTask: m.currentTask || 'Standby',
      } as Machine;
    });

    if (nextMachines.length > 0) {
      setMachines(nextMachines);
    }
  }, []);

  const fetchQueue = useCallback(async () => {
    const response = await fetch('/api/production/queue');
    if (!response.ok) return;

    const payload = await response.json() as {
      currentOrder: { orderId: number; createdAt: string; status: QueueOrder['status']; itemTrayIds: number[] } | null;
      queuedOrders: Array<{ orderId: number; createdAt: string; status: QueueOrder['status']; itemTrayIds: number[] }>;
    };

    const mapOrder = (order: { orderId: number; createdAt: string; status: QueueOrder['status']; itemTrayIds: number[] }): QueueOrder => ({
      orderId: order.orderId,
      createdAt: new Date(order.createdAt),
      status: order.status,
      itemTrayIds: order.itemTrayIds,
    });

    setCurrentOrder(payload.currentOrder ? mapOrder(payload.currentOrder) : null);
    setQueue((payload.queuedOrders ?? []).map(mapOrder));
  }, []);

  const fetchLogs = useCallback(async () => {
    const response = await fetch('/api/production/logs?limit=200');
    if (!response.ok) return;

    const payload = await response.json() as {
      logs?: Array<{
        id: string;
        timestamp: string;
        level: string;
        source: string;
        type: string;
        description: string;
      }>;
    };

    setLogs((payload.logs ?? []).map((l) => ({
      id: l.id,
      timestamp: new Date(l.timestamp),
      level: normalizeLogLevel(l.level),
      source: l.source || 'system',
      type: l.type || 'event',
      description: l.description || '',
    })));
  }, [normalizeLogLevel]);

  // ── Auth ──────────────────────────────────────────────────────────────────

  const login = useCallback((email: string, password: string): boolean => {

     const VALID_EMAIL = 'operator@example.com';
  const VALID_PASSWORD = '1234';

    if (email === VALID_EMAIL && password === VALID_PASSWORD) {
      setIsAuthenticated(true);
      window.localStorage.setItem(OPERATOR_AUTH_STORAGE_KEY, 'true');
      addLog('success', 'Auth', 'Login', `Operator logged in: ${email}`);
      return true;
    }
    return false;
  }, [addLog]);

  const logout = useCallback(() => {
    setIsAuthenticated(false);
    window.localStorage.removeItem(OPERATOR_AUTH_STORAGE_KEY);
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

  useEffect(() => {
    void fetchMachines();
    void fetchQueue();
    void fetchLogs();

    const machinesInterval = window.setInterval(() => {
      void fetchMachines();
    }, 2000);

    const queueInterval = window.setInterval(() => {
      void fetchQueue();
    }, 2000);

    const logsInterval = window.setInterval(() => {
      void fetchLogs();
    }, 5000);

    return () => {
      clearInterval(machinesInterval);
      clearInterval(queueInterval);
      clearInterval(logsInterval);
    };
  }, [fetchLogs, fetchMachines, fetchQueue]);

  useEffect(() => {
    if (!currentOrder) return;

    const syncFlowFromCurrentOrder = async () => {
      const response = await fetch(`/api/production/order-status/${currentOrder.orderId}`);
      if (!response.ok) return;

      const snapshot = await response.json() as {
        stage: ProductionStage;
        state: StageState;
        message?: string;
      };

      setProductionFlow(flowFromSnapshot(snapshot.stage, snapshot.state));
      setStatusMessage(snapshot.message ?? `${snapshot.stage} ${snapshot.state}`);
    };

    void syncFlowFromCurrentOrder();
    const intervalId = window.setInterval(() => {
      void syncFlowFromCurrentOrder();
    }, 2000);

    return () => {
      clearInterval(intervalId);
    };
  }, [currentOrder?.orderId]);
  
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

      addLog(
        normalizeLogLevel(e.Level),
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

        let nextState: StageState | null = null;
        if (stateText === 'in-progress') nextState = 'in-progress';
        else if (stateText === 'completed') nextState = 'completed';
        else if (stateText === 'pending') nextState = 'pending';
        else if (stateText === 'error') nextState = 'error';
        else if (stateText === 'done') nextState = 'completed';

        if (!nextState)
          return;

        setProductionFlow(prev => updateStage(prev, stage, nextState));
        setStatusMessage(message || `${stage} ${nextState}`);

        if (stage === 'delivery' && nextState === 'completed') {
          setSafeTimeout(() => {
            setProductionFlow({ ...emptyFlow });
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
  }, [addLog, normalizeLogLevel]);

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
