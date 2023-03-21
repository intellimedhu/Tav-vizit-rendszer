<template>
  <div id="app">
    <input type="search"
           class="form-control mt-2 mb-4 text-center"
           placeholder="Keresés: település vagy irányítószám"
           autofocus="autofocus"
           @keyup="setSearch" />

    <b-button size="sm" class="mr-1 mb-4" variant="success" v-b-modal.modal-edit @click="createSettlement()">Hozzáad</b-button>

    <div class="table-responsive">
      <b-table striped
               hover
               :items="settlements"
               :fields="fields">
        <template slot="actions" slot-scope="row">
          <b-button size="sm" class="mr-1" variant="primary" v-b-modal.modal-edit @click="setSettlement(row.item)">Szerkesztés</b-button>
          <b-button size="sm" class="mr-1" variant="danger" @click="deleteSettlement(row.item.id)">Törlés</b-button>
        </template>
      </b-table>
    </div>

    <b-pagination
          v-model="pagiantion.currentPage"
          :total-rows="totalCount"
          :per-page="pagiantion.perPage"
          class="my-2"
          size="sm"
          align="center"
          :limit="20"
          @change="paginationChanged"></b-pagination>

    <b-modal id="modal-edit" title="Település megadása" @ok="save">
      <div class="row form-group">
        <label for="zip" class="col-form-label col">Irányítószám</label>
        <div class="col-12">
          <input type="number" id="zip" min="1000" max="9999" class="form-control" v-model="currentSettlement.zipCode" />
          <div v-if="submitted && validation.zipCode" class="text-danger">Helytelen irányítószám</div>
        </div>
      </div>

      <div class="row form-group">
        <label for="name" class="col-form-label col">Település</label>
        <div class="col-12">
          <input type="text" id="name" class="form-control" v-model="currentSettlement.name" />
          <div v-if="submitted && validation.name" class="text-danger">Kötelező</div>
        </div>
      </div>

      <div class="row">
        <label for="description" class="col-form-label col">Egyéb (utca, településrész, stb...)</label>
        <div class="col-12">
          <input type="text" id="description" class="form-control" v-model="currentSettlement.description" />
        </div>
      </div>
    </b-modal>
  </div>
</template>

<script>
export default {
  name: "app",
  data() {
    return {
      settlements: [],
      totalCount: null,
      fields: [
        { key: "zipCode", label: "Irányítószám" },
        { key: "name", label: "Település" },
        { key: "description", label: "Egyéb" },
        { key: "actions", label: "Műveletek" }
      ],
      pagiantion: {
        perPage: 10,
        currentPage: 1
      },
      q: null,
      searchTimeout: null,
      currentSettlement: {
        id: 0,
        zipCode: null,
        description: null,
        name: null
      },
      validation: {
        zipCode: null,
        name: null
      },
      submitted: false
    };
  },
  methods: {
    initialize() {
      this.getSettlements();
    },
    getSettlements() {
      this.$http
        .get(this.apiUrl, {
          params: {
            page: this.pagiantion.currentPage - 1,
            q: this.q
          }
        })
        .then(response => {
          this.settlements = response.data.settlements;
          this.totalCount = response.data.totalCount;
        });
    },
    paginationChanged(page) {
      this.$nextTick(this.getSettlements);
    },
    setSearch(e) {
      clearTimeout(this.searchTimeout);

      this.searchTimeout = setTimeout(() => {
        if (this.q != e.target.value) {
          this.q = e.target.value;
          this.getSettlements();
        }
      }, 500);
    },
    setSettlement(settlement) {
      Object.assign(this.currentSettlement, settlement);
    },
    createSettlement() {
      Object.assign(this.currentSettlement, {
        id: 0,
        zipCode: null,
        description: null,
        name: null
      });
    },
    save(e) {
      this.submitted = true;
      this.validation.zipCode = !/^\d{4}$/.test(this.currentSettlement.zipCode);
      this.validation.name = !this.currentSettlement.name;
      if (this.validation.zipCode || this.validation.name) {
        e.preventDefault();
        return;
      }

      this.submitted = false;
      this.$http.post(this.apiUrl, this.currentSettlement).then(e => {
        if (e.status == 200) {
          this.getSettlements();
          alert("Sikeres mentés");
        }
      });
    },
    deleteSettlement(id) {
      if (!confirm("Biztos, hogy törölni szeretné?")) {
        return;
      }

      this.$http
        .delete(this.apiUrl, {
          params: {
            id: id
          }
        })
        .then(e => {
          if (e.status == 200) {
            this.getSettlements();
            alert("Sikeres törlés");
          }
        });
    }
  },
  mounted() {
    this.initialize();
  },
  props: {
    apiUrl: String
  }
};
</script>