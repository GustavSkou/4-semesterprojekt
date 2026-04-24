export type StageState = 'completed' | 'in-progress' | 'pending' | 'error';

export type ProductionStage =
  | 'website'
  | 'warehouse-receive'
  | 'agv-to-assembly'
  | 'assembly'
  | 'agv-to-warehouse'
  | 'warehouse-delivery'
  | 'delivery';

export interface ProductionFlow {
  [key: string]: StageState;
}

export interface ComputerOrder {
  id: string;
  cpu: string;
  gpu: string;
  ram: string;
  storage: string;
  motherboard: string;
  powerSupply: string;
  case: string;
  createdAt: Date;
}

export type QueueOrderStatus = 'pending' | 'in-progress' | 'paused';

export interface QueueOrder {
  orderId: number;
  createdAt: Date;
  status: QueueOrderStatus;
  itemTrayIds: number[];
}

export interface QueueSnapshot {
  currentOrder: QueueOrder | null;
  queuedOrders: QueueOrder[];
}

export type MachineState = 'idle' | 'working' | 'error' | 'offline';

export interface Machine {
  id: string;
  name: string;
  type: 'warehouse' | 'agv' | 'assembly';
  status: 'connected' | 'disconnected';
  state: MachineState;
  currentTask: string;
}

export type LogLevel = 'info' | 'warning' | 'error' | 'success';

export interface ProductionLog {
  id: string;
  timestamp: Date;
  level: LogLevel;
  source: string;
  type: string;
  description: string;
}

export interface OrderStatusSnapshot {
  orderId: number;
  stage: ProductionStage;
  state: StageState;
  message: string;
  updatedAt: Date;
}

export type ProductionStatus = 'running' | 'stopped' | 'reset';
