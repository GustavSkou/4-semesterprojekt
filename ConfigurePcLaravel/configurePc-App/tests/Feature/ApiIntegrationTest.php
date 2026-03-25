<?php

  namespace Tests\Feature;

  use Illuminate\Foundation\Testing\RefreshDatabase;
  use Illuminate\Support\Facades\Http;
  use Tests\TestCase;

  class ApiIntegrationTest extends TestCase
  {
      use RefreshDatabase;

      // Test 1: "Can we open the front page?"
      // We send a GET request to "/" and expect a 200 OK response.
      public function test_spa_shell_is_available(): void
      {
          $this->get('/')->assertStatus(200);
      }

      // Test 2: "Can we fetch components?"
      // We seed the database, then call the API and check we get a JSON list back.
      public function test_components_endpoint_returns_data(): void
      {
          $this->seed(); // bruger DatabaseSeeder

          $this->getJson('/api/components')
              ->assertStatus(200)
              ->assertJsonIsArray();
      }

      // Test 3: "Does the command endpoint accept valid input?"
      // We send a command and expect the same data back in the response.
      public function test_command_endpoint_validates_and_echoes(): void
      {
          $this->postJson('/api/production/command', [
              'command' => 'start',
              'parameters' => ['speed' => 1],
          ])
          ->assertStatus(200)
          ->assertJson([
              'message' => 'Command received successfully',
              'command' => 'start',
              'parameters' => ['speed' => 1],
          ]);
      }

      // Test 4: "Does the orders endpoint forward data to the production API?"
      // We fake the external API, send an order, and expect the fake response back.
      public function test_order_endpoint_forwards_to_production_api(): void
      {
          $this->setProductionApiUrl('http://example.test');
          Http::fake([
              '*' => Http::response(['ok' => true], 200),
          ]);

          $this->postJson('/api/orders', [
              'id' => 1,
              'trayIds' => [10, 11],
            ])
            ->assertStatus(200)
            ->assertJson(['ok' => true]);
        }

      // Test 5: "Mangler vi command?"
      // Vi sender en tom request og forventer en valideringsfejl (422).
      public function test_command_requires_command(): void
      {
          $this->postJson('/api/production/command', [])
              ->assertStatus(422)
              ->assertJsonValidationErrors(['command']);
      }

      // Test 6: "Mangler vi trayIds?"
      // Vi sender kun id og forventer en valideringsfejl (422).
      public function test_orders_requires_tray_ids(): void
      {
          $this->postJson('/api/orders', ['id' => 1])
              ->assertStatus(422)
              ->assertJsonValidationErrors(['trayIds']);
      }


      // Test 7: "Skal trayIds vaere tal?"
      // Vi sender tekst i stedet for tal og forventer en valideringsfejl.
      public function test_orders_requires_integer_tray_ids(): void
      {
          $this->postJson('/api/orders', ['id' => 1, 'trayIds' => ['x']])
              ->assertStatus(422)
              ->assertJsonValidationErrors(['trayIds.0']);
      }

      // Test 8: "Sender vi rigtigt data videre?"
      // Vi faker den eksterne API og tjekker at payload er korrekt.
      public function test_orders_forwards_correct_payload(): void
      {
          $this->setProductionApiUrl('http://example.test');
          Http::fake();

          $this->postJson('/api/orders', ['id' => 1, 'trayIds' => [10, 11]])
              ->assertStatus(200);

          Http::assertSent(function ($request) {
              return str_ends_with($request->url(), '/ProductionSystem/Command')
                  && $request['Name'] === 'order'
                  && $request['Parameters']['id'] === 1
                  && $request['Parameters']['items'] === [10, 11];
          });
      }

      // Lille helper: Saetter env-variablen, saa testen kan finde URL'en.
      private function setProductionApiUrl(string $url): void
      {
          putenv("PRODUCTION_API_URL={$url}");
          $_ENV['PRODUCTION_API_URL'] = $url;
          $_SERVER['PRODUCTION_API_URL'] = $url;
      }


    }
    
