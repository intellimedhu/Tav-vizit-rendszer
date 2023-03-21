import axios from 'axios';

const service = {
  getCenterProfile(url, params) {
    return axios.get(url, {
      params: params
    });
  },

  searchByZipCode(url, zipCode) {
    return axios.get(url, {
      params: {
        zipCode: zipCode
      }
    });
  },

  submit(url, data) {
    return axios.post(url, data);
  },

  async getColleagueProfileAsync(url, colleague) {
    const response = await axios
      .get(url, {
        params: {
          id: colleague.id
        }
      });

    return service.formatProfileResponse(response.data, colleague);
  },

  async getLeaderProfileAsync(url, memberRightId) {
    const response = await axios
      .get(url, {
        params: {
          memberRightId: memberRightId
        }
      });

    return service.formatProfileResponse(response.data, {});
  },

  formatProfileResponse(responseData, colleague) {
    return Object.assign({}, colleague, responseData, {
      qualifications: responseData.personQualifications.qualifications.map(x => {
        let qualification = responseData.qualifications.find(q => q.id == x.id);

        return Object.assign({}, x, {
          qualification: qualification ? qualification.name : ''
        });
      })
    });
  }
};


export default service;