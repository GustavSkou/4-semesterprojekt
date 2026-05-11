import { act, render, screen, waitFor } from '@testing-library/react';
import React from 'react';
import { vi } from 'vitest';
import { ProductionProvider, useProduction } from './ProductionContext';

class MockEventSource {
  static instances: MockEventSource[] = [];

  onmessage: ((event: MessageEvent<string>) => void) | null = null;
  onerror: (() => void) | null = null;
  url: string;

  constructor(url: string) {
    this.url = url;
    MockEventSource.instances.push(this);
  }

  emitMessage(payload: unknown) {
    this.onmessage?.({
      data: JSON.stringify(payload),
    } as MessageEvent<string>);
  }

  emitError() {
    this.onerror?.();
  }

  close() {
    return undefined;
  }
}

function ProductionHarness() {
  const { statusMessage, productionFlow, machines } = useProduction();
  const warehouse = machines.find(machine => machine.type === 'warehouse');

  return (
    <div>
      <div data-testid="status-message">{statusMessage}</div>
      <div data-testid="website-stage">{productionFlow.website}</div>
      <div data-testid="warehouse-status">{warehouse?.status}</div>
      <div data-testid="warehouse-task">{warehouse?.currentTask}</div>
    </div>
  );
}

describe('ProductionContext functional requirements', () => {
  beforeEach(() => {
    MockEventSource.instances = [];
    vi.stubGlobal('EventSource', MockEventSource);
  });

  afterEach(() => {
    vi.unstubAllGlobals();
  });

  test('F08_updates_operator_state_immediately_when_real_time_status_events_arrive', async () => {
    render(
      <ProductionProvider>
        <ProductionHarness />
      </ProductionProvider>
    );

    const stream = MockEventSource.instances[0];
    expect(stream.url).toBe('http://localhost:5027/ProductionSystem/Events');

    act(() => {
      stream.emitMessage({
        DateAndTime: '2026-05-11T12:00:00Z',
        Description: 'website|in-progress|Order 123 received',
        Source: 'Warehouse Controller',
        Type: 'step-status',
        Level: 'low',
      });
    });

    await waitFor(() => {
      expect(screen.getByTestId('website-stage')).toHaveTextContent('in-progress');
    });
    expect(screen.getByTestId('status-message')).toHaveTextContent('Order 123 received');
    expect(screen.getByTestId('warehouse-task')).toHaveTextContent('website|in-progress|Order 123 received');
  });
});
